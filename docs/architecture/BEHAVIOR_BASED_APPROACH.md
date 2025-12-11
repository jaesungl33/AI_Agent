# Behavior-Based Coverage Evaluation

## 🎯 Overview

This system has been transformed from a direct GDD→Code matching approach to a **QA-style behavior-based approach**. This eliminates scalability issues, reduces LLM calls, and provides more accurate coverage evaluation.

## 🔄 The Transformation

### ❌ Old Approach (Not Scalable)
1. GDD → Short summary
2. Summary → Code function name
3. Match to 1000+ code files
4. **Crash** due to too many LLM calls, embeddings, semantic load

### ✅ New Approach (Scalable)
1. **GDD → Behavior Requirements** (Structured: triggers, effects, entities, conditions)
2. **Codebase → Behavior Index** (Extract methods, convert to behavior descriptions)
3. **Match Behaviors** (Requirement behaviors ↔ Code behaviors via embeddings)
4. **LLM Final Decision** (Only on top 3-5 matches)

## 📋 Architecture

### Step 1: Extract Behavior Requirements
**File:** `gdd_rag_backbone/gdd/extraction.py`

- `convert_to_behavior_requirement()`: Converts GDD requirement to structured behavior
- `extract_behavior_requirements()`: Batch extraction from GDD

**Schema:** `BehaviorRequirement`
```python
{
  "id": "HI_invisibility",
  "summary": "Player becomes invisible in hiding objects",
  "triggers": ["Player enters collider of HI_* object"],
  "effects": ["Set player visibility = false"],
  "entities": ["Player", "HI_Grass", "Camera"],
  "conditions": [],
  "priority": "high"
}
```

### Step 2: Index Code Behaviors
**File:** `gdd_rag_backbone/gdd/behavior_indexing.py`

- `index_code_behaviors()`: Extracts all methods from codebase
- `extract_code_behavior()`: Converts each method to behavior description via LLM
- `save_behavior_index()` / `load_behavior_index()`: Cache behavior index

**Schema:** `CodeBehavior`
```python
{
  "symbol": "HidingGrass.OnTriggerEnter",
  "description": "Handles entering grass, sets player invisible",
  "trigger_patterns": ["OnTriggerEnter", "enter hiding object"],
  "effect_patterns": ["player invisible", "stealth mode"],
  "entities": ["Player", "Grass", "HidingSystem"]
}
```

### Step 3: Match Behaviors
**File:** `gdd_rag_backbone/gdd/behavior_matching.py`

- `find_matching_behaviors()`: Semantic similarity between requirement and code behaviors
- `batch_find_matching_behaviors()`: Efficient batch matching
- Uses embeddings + cosine similarity (fast, lightweight)

### Step 4: Final LLM Classification
**File:** `gdd_rag_backbone/gdd/requirement_matching.py`

- `evaluate_requirement_behavior()`: Main entry point for behavior-based evaluation
- `evaluate_all_requirements_behavior()`: Batch evaluation
- `classify_behavior_implementation()`: LLM only called on top 3-5 matches

## 🚀 Usage

### Basic Usage

```python
from gdd_rag_backbone.gdd import (
    extract_all_requirements,
    evaluate_all_requirements_behavior,
)

# Extract requirements from GDD
all_data = await extract_all_requirements(doc_id="my_gdd")

# Evaluate using behavior-based approach
report_path = await evaluate_all_requirements_behavior(
    doc_id="my_gdd",
    code_index_id="tank_online_codebase",
    requirements=all_data["requirements"],
)
```

### Advanced Usage

```python
from gdd_rag_backbone.gdd import (
    convert_to_behavior_requirement,
    index_code_behaviors,
    find_matching_behaviors,
    evaluate_requirement_behavior,
)

# Step 1: Convert requirement to behavior
behavior_req = await convert_to_behavior_requirement(gdd_requirement)

# Step 2: Index code behaviors (cached after first run)
code_behaviors = await index_code_behaviors("tank_online_codebase")

# Step 3: Find matches
matches = await find_matching_behaviors(behavior_req, code_behaviors, top_k=5)

# Step 4: Evaluate (includes LLM classification on top matches)
result = await evaluate_requirement_behavior(
    gdd_requirement,
    "tank_online_codebase",
    code_behaviors=code_behaviors,
)
```

## 📊 Benefits

1. **FAST**: Embeddings on tiny behavior summaries, not entire code files
2. **STABLE**: Fewer LLM calls → no backend timeout
3. **ACCURATE**: QA-style requirements match code better
4. **SCALABLE**: Works for 10k or 100k lines of code
5. **HUMAN-LIKE**: Matches real QA workflow

## 🔑 Key Insight

**You are not building a system that checks code directly.**
**You're building a system that checks implementation of behavior.**

Behavior is the bridge between GDD and code.

**Stop linking:** GDD <---> Code  
**Start linking:** GDD --> Behavior Requirements <---> Code Behavior

## 📁 Files Created/Modified

### New Files
- `gdd_rag_backbone/gdd/behavior_indexing.py` - Code behavior extraction
- `gdd_rag_backbone/gdd/behavior_matching.py` - Behavior matching via embeddings

### Modified Files
- `gdd_rag_backbone/gdd/schemas.py` - Added `BehaviorRequirement` and `CodeBehavior`
- `gdd_rag_backbone/gdd/extraction.py` - Added behavior requirement conversion
- `gdd_rag_backbone/gdd/requirement_matching.py` - Added behavior-based evaluation functions
- `gdd_rag_backbone/gdd/__init__.py` - Exported new functions

## 🎓 Example Workflow

1. **GDD Requirement:**
   ```
   "Player should become invisible when entering grass objects"
   ```

2. **Behavior Requirement (extracted):**
   ```json
   {
     "triggers": ["Player enters collider of HI_Grass object"],
     "effects": ["Set player visibility = false"],
     "entities": ["Player", "HI_Grass"]
   }
   ```

3. **Code Behavior (extracted from method):**
   ```json
   {
     "symbol": "HidingGrass.OnTriggerEnter",
     "trigger_patterns": ["OnTriggerEnter", "enter"],
     "effect_patterns": ["player invisible"],
     "entities": ["Player", "Grass"]
   }
   ```

4. **Match:** High similarity (0.85) via embeddings

5. **LLM Classification:** "implemented" (only called on this top match)

## ⚠️ Notes

- Behavior index is cached in `rag_storage/behavior_indices/` after first creation
- Method extraction supports both Python (`def`) and C# (`public/private/protected`)
- Embeddings are cached in-memory for performance
- LLM is only called on top 3-5 matches (configurable via `top_k`)


