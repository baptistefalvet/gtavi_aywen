---
model: claude-sonnet-4-5-20250929
---

# Gather Context on Feature

Explore and understand a specific feature of the game before discussion or planning.

**Usage**:
- `/gather-context tank-movement` - Gather context on "tank-movement" feature
- `/gather-context "input system"` - Gather context on "input system" feature

---

## Instructions

We are going to work on the **{arg}** of the game. Dig in, read relevant files, and prepare to discuss the ins and outs of how it works. ultrathink.

### 1. Search for Relevant Documentation

Search the project for references to the feature:
- Check `Assets/_Docs/GDD/` for game design documentation related to **{arg}**
- Check `Assets/_Docs/Stories/` for any user stories related to **{arg}**
- Search for related C# scripts, prefabs, and scenes

### 2. Explore Implementation

Read and analyze:
- Related C# scripts (classes, managers, controllers...)
- Related GameObjects and their component hierarchies
- Existing configuration or data structures

### 3. Map Out Current State

Document what you find:
- What currently exists (scripts, components, game objects)
- How it's implemented (architecture, patterns used) 
- What files are involved
- Key functions and their purposes

### 4. Summarize Your Findings

Present a clear overview including:
- **What it is**: Brief description of the feature
- **How it works**: High-level flow and interactions
- **Current implementation**: Files involved, key classes/methods
- **Architecture**: Design patterns used, event-driven aspects
- **Ready for discussion**: "I'm ready to discuss [feature] in detail"

---

## Rules

**Do**:
- Read game design documents first
- Explore the actual code implementation
- Check both editor setup and script code
- Be thorough but concise in summary

**Don't**:
- Make changes or create new files
- Write implementation code
- Skip documentation review
