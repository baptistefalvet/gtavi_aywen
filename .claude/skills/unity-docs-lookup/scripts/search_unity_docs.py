#!/usr/bin/env python3
"""
Search Unity documentation efficiently.
Auto-detects Unity installation and extracts relevant API info.
Supports both core Unity docs and package documentation.
"""

import sys
import os
import re
from pathlib import Path

def find_unity_docs_path():
    """Find the latest Unity installation's documentation path."""
    unity_hub = Path("/Applications/Unity/Hub/Editor")

    if not unity_hub.exists():
        return None

    # Get all Unity versions, sort to find latest
    versions = sorted(unity_hub.iterdir(), reverse=True)

    for version_dir in versions:
        docs_path = version_dir / "Documentation" / "en"
        if docs_path.exists():
            return docs_path

    return None

def find_project_root():
    """Find the Unity project root by looking for Library/PackageCache."""
    current = Path.cwd()

    # Try current directory and parents
    for path in [current] + list(current.parents):
        package_cache = path / "Library" / "PackageCache"
        if package_cache.exists():
            return path

    return None

def clean_html(text):
    """Remove HTML tags and clean up whitespace."""
    # Remove script/style blocks
    text = re.sub(r'<script[^>]*>.*?</script>', '', text, flags=re.DOTALL)
    text = re.sub(r'<style[^>]*>.*?</style>', '', text, flags=re.DOTALL)
    # Remove HTML tags
    text = re.sub(r'<[^>]+>', ' ', text)
    # Clean up whitespace
    text = re.sub(r'\s+', ' ', text)
    # Decode common HTML entities
    text = text.replace('&lt;', '<').replace('&gt;', '>').replace('&amp;', '&')
    return text.strip()

def clean_markdown(text):
    """Clean up markdown text for console output."""
    # Remove image references
    text = re.sub(r'!\[.*?\]\(.*?\)', '', text)
    # Convert links to just text
    text = re.sub(r'\[([^\]]+)\]\([^\)]+\)', r'\1', text)
    # Remove excessive newlines
    text = re.sub(r'\n{3,}', '\n\n', text)
    return text.strip()

def extract_markdown_section(content, section_name):
    """Extract a section from markdown content."""
    # Look for heading with section name
    pattern = rf'^##+ ({section_name}).*?\n(.*?)(?=^##+ |\Z)'
    match = re.search(pattern, content, re.MULTILINE | re.DOTALL | re.IGNORECASE)
    if match:
        return match.group(2).strip() if match.group(2) else None
    return None

def parse_markdown_table(table_text):
    """Parse a markdown table and extract key information."""
    lines = table_text.split('\n')
    rows = []

    for line in lines:
        if '|' in line and not line.strip().startswith('|:'):  # Skip separator lines
            cells = [cell.strip() for cell in line.split('|')[1:-1]]  # Remove empty first/last
            if cells and cells[0] and not cells[0].startswith('-'):
                rows.append(cells)

    return rows

def search_package_docs(project_root, class_name):
    """Search PackageCache for markdown documentation."""
    if not project_root:
        return None

    package_cache = project_root / "Library" / "PackageCache"
    if not package_cache.exists():
        return None

    # Search all Unity packages
    results = []
    for package_dir in sorted(package_cache.glob("com.unity.*"), reverse=True):
        docs_dir = package_dir / "Documentation~"
        if not docs_dir.exists():
            continue

        # Search for matching markdown files
        for md_file in docs_dir.glob("*.md"):
            # Check if filename matches (case-insensitive)
            if class_name.lower() in md_file.stem.lower():
                try:
                    content = md_file.read_text(errors='ignore')
                    package_name = package_dir.name
                    results.append({
                        'file': md_file,
                        'package': package_name,
                        'content': content
                    })
                except Exception:
                    continue

    return results if results else None

def display_package_doc(doc_info):
    """Display package documentation in a readable format."""
    package = doc_info['package']
    content = doc_info['content']
    file_path = doc_info['file']

    print(f"\n{'='*60}")
    print(f"Found in package: {package}")
    print(f"File: {file_path.name}")
    print(f"{'='*60}\n")

    # Extract title
    title_match = re.search(r'^# (.+)$', content, re.MULTILINE)
    if title_match:
        print(f"{title_match.group(1)}\n")

    # Extract description (first paragraph after title)
    desc_match = re.search(r'^# .+?\n\n(.+?)(?=\n\n|\n#)', content, re.MULTILINE | re.DOTALL)
    if desc_match:
        desc = clean_markdown(desc_match.group(1))
        print(f"{desc}\n")

    # Extract Properties section
    props_section = extract_markdown_section(content, "Properties|Targets")
    if props_section:
        print("## Properties\n")

        # Parse table if present
        if '|' in props_section:
            rows = parse_markdown_table(props_section)
            for row in rows[:10]:  # Limit output
                if len(row) >= 2:
                    prop_name = row[0].replace('**', '').strip()
                    prop_desc = clean_markdown(' '.join(row[1:])).strip()
                    if prop_name and prop_desc:
                        print(f"  {prop_name}")
                        print(f"    {prop_desc[:200]}{'...' if len(prop_desc) > 200 else ''}\n")
        else:
            # Just print the section
            print(clean_markdown(props_section)[:500])
            print()

    # Look for specific important sections
    for section_name in ["Targets", "Usage", "Examples", "Important"]:
        section = extract_markdown_section(content, section_name)
        if section and section != props_section:  # Don't duplicate
            print(f"## {section_name}\n")
            cleaned = clean_markdown(section)
            print(f"{cleaned[:400]}{'...' if len(cleaned) > 400 else ''}\n")

    print(f"\nFull documentation: {file_path}")
    print(f"{'='*60}\n")

def lookup_api(docs_path, class_name, member_name=None):
    """Look up Unity API documentation."""
    script_ref = docs_path / "ScriptReference"

    if member_name:
        # Try member-specific file first (ClassName.MemberName.html)
        member_file = script_ref / f"{class_name}.{member_name}.html"
        if member_file.exists():
            html = member_file.read_text(errors='ignore')

            print(f"\n=== {class_name}.{member_name} (Core Unity API) ===\n")

            # Extract description
            desc_match = re.search(r'<h3>Description</h3>\s*<p>(.*?)</p>', html, re.DOTALL)
            if desc_match:
                print(f"Description: {clean_html(desc_match.group(1))}\n")

            # Extract signature/declaration
            sig_match = re.search(r'<div class="signature[^"]*"[^>]*>(.*?)</div>', html, re.DOTALL)
            if sig_match:
                sig = clean_html(sig_match.group(1))
                if sig:
                    print(f"Signature: {sig}\n")

            # Extract parameters
            params_match = re.search(r'<h4>Parameters</h4>(.*?)(?:<h[34]>|</div>)', html, re.DOTALL)
            if params_match:
                params = clean_html(params_match.group(1))
                print(f"Parameters: {params}\n")

            # Extract returns
            returns_match = re.search(r'<h4>Returns</h4>\s*<p>(.*?)</p>', html, re.DOTALL)
            if returns_match:
                print(f"Returns: {clean_html(returns_match.group(1))}\n")

            return True

    # Fall back to class file
    class_file = script_ref / f"{class_name}.html"
    if class_file.exists():
        html = class_file.read_text(errors='ignore')

        print(f"\n=== {class_name} (Core Unity API) ===\n")

        # Extract class description
        desc_match = re.search(r'<h3>Description</h3>\s*<p>(.*?)</p>', html, re.DOTALL)
        if desc_match:
            print(f"{clean_html(desc_match.group(1))}\n")

        if member_name:
            # Search for member within class file
            member_pattern = rf'{member_name}.*?</tr>'
            matches = re.findall(member_pattern, html, re.DOTALL | re.IGNORECASE)
            if matches:
                print(f"Found references to '{member_name}':")
                for m in matches[:3]:
                    print(f"  - {clean_html(m)[:100]}")
            else:
                print(f"Member '{member_name}' not found. Check spelling or try --search.")
        else:
            # List properties and methods
            props = re.findall(r'<td class="lbl"><a href="[^"]*">(\w+)</a></td>', html)
            if props:
                print(f"Members: {', '.join(props[:15])}")
                if len(props) > 15:
                    print(f"  ... and {len(props) - 15} more")

        return True

    # Not found in core Unity docs
    return False

def search_docs(docs_path, term, project_root=None):
    """Search for classes/members matching a term."""
    script_ref = docs_path / "ScriptReference"

    # Find matching files in core Unity docs
    matches = list(script_ref.glob(f"*{term}*.html"))
    matches = [m.stem for m in matches if not m.stem.startswith('10')]  # Filter numeric prefixes
    matches = sorted(set(matches))[:20]

    # Search package docs
    package_matches = []
    if project_root:
        package_cache = project_root / "Library" / "PackageCache"
        if package_cache.exists():
            for package_dir in sorted(package_cache.glob("com.unity.*"), reverse=True):
                docs_dir = package_dir / "Documentation~"
                if docs_dir.exists():
                    for md_file in docs_dir.glob(f"*{term}*.md"):
                        package_matches.append(f"{md_file.stem} ({package_dir.name})")

    if matches:
        print(f"\n=== Core Unity API ({len(matches)} matches) ===\n")
        for m in matches:
            print(f"  {m}")

    if package_matches:
        print(f"\n=== Package Documentation ({len(package_matches)} matches) ===\n")
        for m in package_matches[:20]:
            print(f"  {m}")

    if not matches and not package_matches:
        print(f"No matches found for '{term}'")

def main():
    if len(sys.argv) < 2 or sys.argv[1] in ['-h', '--help']:
        print("Usage:")
        print("  python3 search_unity_docs.py <ClassName> [MemberName]")
        print("  python3 search_unity_docs.py --search <term>")
        print("\nExamples:")
        print("  python3 search_unity_docs.py Transform")
        print("  python3 search_unity_docs.py Rigidbody AddForce")
        print("  python3 search_unity_docs.py CinemachineCamera  # Package docs")
        print("  python3 search_unity_docs.py --search Input")
        sys.exit(0)

    docs_path = find_unity_docs_path()
    if not docs_path:
        print("Error: Unity documentation not found.")
        print("Expected at: /Applications/Unity/Hub/Editor/*/Documentation/en")
        sys.exit(1)

    # Find project root for package docs
    project_root = find_project_root()

    if sys.argv[1] == '--search':
        if len(sys.argv) < 3:
            print("Error: --search requires a term")
            sys.exit(1)
        search_docs(docs_path, sys.argv[2], project_root)
    else:
        class_name = sys.argv[1]
        member_name = sys.argv[2] if len(sys.argv) > 2 else None

        # Try core Unity API first
        found = lookup_api(docs_path, class_name, member_name)

        # If not found, search package docs
        if not found and project_root:
            package_docs = search_package_docs(project_root, class_name)
            if package_docs:
                for doc in package_docs:
                    display_package_doc(doc)
                found = True

        if not found:
            print(f"Error: '{class_name}' not found in Unity documentation or packages.")
            print("Try using --search to find similar classes.")
            sys.exit(1)

if __name__ == "__main__":
    main()
