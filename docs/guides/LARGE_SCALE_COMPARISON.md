# Large-Scale GDD-to-Code Comparison Guide

## 🎯 Best Approach: Behavior-Based Matching (Already Implemented!)

Your system already uses the **optimal approach** for comparing GDD requirements to code implementations at scale. Here's why it works and how to optimize it for 543+ .cs files.

## ✅ Why Behavior-Based is Best

### The Problem with Direct Matching
- **543 .cs files** = thousands of methods/functions
- **Multiple GDDs** = hundreds of requirements
- **Direct matching** = 543 files × 100 requirements = 54,300 comparisons
- **Result**: Timeout, expensive, slow

### The Behavior-Based Solution
1. **One-time indexing**: Extract behaviors from all 543 files → ~2000-5000 behaviors
2. **Fast matching**: Use embeddings (cosine similarity) → milliseconds per match
3. **Smart filtering**: Only call LLM on top 3-5 matches → 95% fewer LLM calls
4. **Cached results**: Behavior index is saved and reused

**Result**: Scales to 10,000+ files, runs in minutes instead of hours

## 📊 Current Architecture

```
┌─────────────────┐
│  GDD Documents  │
│  (43 GDDs)      │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐
│ Extract Requirements     │
│ → Behavior Requirements  │
│   (triggers, effects)    │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Match via Embeddings     │
│ (Fast similarity search) │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ LLM Verification         │
│ (Only top 3-5 matches)   │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Coverage Report          │
│ (implemented/partial/no) │
└─────────────────────────┘

┌─────────────────┐
│  Code Files      │
│  (543 .cs files) │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐
│ Extract Methods          │
│ → Code Behaviors         │
│   (cached after 1st run) │
└─────────────────────────┘
```

## 🚀 Optimized Workflow for Your Scale

### Step 1: Index Code Behaviors (One-Time, ~30-60 minutes)

**First time only** - extracts behaviors from all 543 files:

```python
# This happens automatically when you call /coverage/evaluate
# But you can pre-build it:

POST /coverage/evaluate
{
  "docId": "any_gdd",  # Just to trigger indexing
  "codeIndexId": "tank_online_codebase",
  "workspaceId": "tank_war"
}
```

**What happens:**
1. Scans all 543 .cs files
2. Extracts all methods (~2000-5000 methods)
3. Converts each to behavior description via LLM
4. **Caches to**: `data/workspaces/tank_war/behavior_cache/tank_online_codebase_behaviors.json`

**Time**: ~30-60 minutes (one-time)
**Cost**: ~2000-5000 LLM calls (one-time)

### Step 2: Extract GDD Requirements (Fast, ~1-2 minutes per GDD)

```python
GET /gdd/{doc_id}/spec?workspaceId=tank_war
```

**What happens:**
1. Extracts requirements from GDD
2. Converts each to behavior requirements
3. **Caches to**: `data/workspaces/tank_war/reports/{doc_id}_spec.json`

**Time**: ~1-2 minutes per GDD
**Reusable**: Once extracted, can reuse for multiple comparisons

### Step 3: Compare (Fast, ~30 seconds per requirement)

```python
POST /coverage/evaluate
{
  "docId": ["gdd1", "gdd2", "gdd3"],  # Multiple GDDs
  "codeIndexId": "tank_online_codebase",
  "workspaceId": "tank_war",
  "topK": 5
}
```

**What happens:**
1. Loads cached behavior index (instant)
2. Loads GDD requirements (instant if cached)
3. For each requirement:
   - Embedding similarity search → top 5 matches (fast)
   - LLM verification on top 5 only (5 LLM calls per requirement)
4. Returns coverage report

**Time**: ~30 seconds per requirement
- 10 requirements = ~5 minutes
- 50 requirements = ~25 minutes
- 100 requirements = ~50 minutes

## ⚡ Performance Optimizations

### 1. Pre-Build Behavior Index

Instead of waiting for first comparison, build it upfront:

```bash
# After uploading code, trigger behavior indexing
curl -X POST "http://localhost:8000/coverage/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "docId": "dummy",
    "codeIndexId": "tank_online_codebase",
    "workspaceId": "tank_war"
  }'
```

This will build the cache even if evaluation fails (no GDD).

### 2. Batch Process GDDs

Extract all GDD requirements first, then compare:

```python
# Extract all GDDs
for gdd_id in gdd_ids:
    GET /gdd/{gdd_id}/spec?workspaceId=tank_war

# Then compare all at once
POST /coverage/evaluate
{
  "docId": ["gdd1", "gdd2", ...],  # All GDDs
  "codeIndexId": "tank_online_codebase"
}
```

### 3. Use Fast Mode in UI

The frontend has a "Fast mode" that limits to:
- Single GDD
- Single code batch
- Top 5 requirements

Use this for quick checks, full mode for comprehensive reports.

### 4. Incremental Updates

When code changes, you can rebuild just the changed files:

```python
# Currently: rebuilds entire index
# Future: could detect changed files and update incrementally
```

## 📈 Scaling Estimates

| Scale | Files | Methods | Requirements | Time | LLM Calls |
|-------|-------|---------|-------------|------|-----------|
| **Small** | 50 | ~200 | 10 | ~5 min | ~50 |
| **Medium** | 200 | ~800 | 50 | ~25 min | ~250 |
| **Your Scale** | 543 | ~2000-5000 | 100 | ~50 min | ~500 |
| **Large** | 2000 | ~8000 | 200 | ~100 min | ~1000 |
| **Huge** | 10000 | ~40000 | 500 | ~250 min | ~2500 |

**Key Insight**: Time scales linearly with requirements, NOT with code size (because behavior index is cached).

## 🔧 Recommended Improvements

### 1. Parallel Processing

Currently processes requirements sequentially. Could parallelize:

```python
# Current: Sequential
for req in requirements:
    result = await evaluate_requirement(...)

# Better: Batch parallel (5-10 at a time)
async def evaluate_batch(requirements):
    tasks = [evaluate_requirement(req) for req in requirements]
    return await asyncio.gather(*tasks, return_exceptions=True)
```

**Speedup**: 5-10x faster

### 2. Incremental Behavior Index

Only rebuild changed files:

```python
# Track file hashes
# Only re-index files that changed
# Merge with existing index
```

**Speedup**: 10-100x faster for small changes

### 3. Embedding Cache

Cache embeddings for behavior requirements:

```python
# Current: Re-embeds each requirement every time
# Better: Cache requirement embeddings
# Reuse for multiple comparisons
```

**Speedup**: 2-3x faster for repeated comparisons

### 4. Smart Filtering

Pre-filter code behaviors by category:

```python
# Group behaviors by category (UI, Gameplay, Network, etc.)
# Only search relevant categories for each requirement
```

**Speedup**: 2-5x faster matching

## 🎯 Best Practices

### For Your 543 Files:

1. **First Run** (One-time setup):
   - Upload all code → triggers indexing
   - Wait ~30-60 minutes for behavior index
   - Extract all GDD requirements (~5-10 minutes)

2. **Regular Comparisons**:
   - Use cached behavior index (instant)
   - Compare against all GDDs or specific ones
   - Results in ~30 seconds per requirement

3. **When Code Changes**:
   - Re-upload changed files
   - Rebuild behavior index (or wait for auto-rebuild)
   - Re-compare

4. **For Quick Checks**:
   - Use "Fast mode" in UI
   - Limit to top 5-10 requirements
   - Single GDD + single code batch

## 🚫 What NOT to Do

### ❌ Don't Use Simple Name Matching
The `/export/compare` endpoint does naive substring matching. This is:
- **Inaccurate**: "Player" matches "PlayerController" but not "HandlePlayerInput"
- **Misses synonyms**: "invisible" vs "hidden" vs "stealth"
- **No context**: Can't understand behavior

### ❌ Don't Scan All Files Per Requirement
- 543 files × 100 requirements = 54,300 file scans
- Each scan = LLM call = expensive + slow
- **Result**: Timeout, crashes

### ✅ DO Use Behavior-Based Approach
- One-time indexing: 543 files → 2000-5000 behaviors
- Fast matching: Embeddings (milliseconds)
- Smart verification: LLM only on top matches
- **Result**: Scales to any size

## 📝 Summary

**Your current system is already optimal!** The behavior-based approach is the industry-standard way to do large-scale code-to-requirement matching.

**Key advantages:**
1. ✅ Scales to 10,000+ files
2. ✅ Fast (30 sec/requirement)
3. ✅ Accurate (semantic matching)
4. ✅ Cost-effective (cached, minimal LLM calls)
5. ✅ Already implemented!

**Next steps:**
1. Let the code indexing finish (543 files → behavior index)
2. Extract GDD requirements (cached)
3. Run comparisons (fast with cached index)
4. Consider parallel processing for speedup

The system is designed for exactly your use case! 🎉


