# Performance Improvements: Code Processing Optimization

## Problem Identified

The code coverage evaluation process was taking too long because:

1. **Sequential Query Processing**: Multiple queries for each item were executed one-by-one
2. **Sequential Item Processing**: All items (objects, systems, logic rules, requirements) were processed one at a time
3. **Sequential LLM Calls**: Each item waited for the previous item's LLM classification to complete

### Example Timeline (Before Optimization)
For 20 items with 3 queries each:
- 20 items × 3 queries × 2 seconds = **120 seconds** for queries
- 20 items × 5 seconds (LLM) = **100 seconds** for classifications
- **Total: ~220 seconds (3.7 minutes)** - all sequential

## Optimizations Applied

### 1. Parallel Query Execution (`search_code_chunks`)
**File**: `gdd_rag_backbone/gdd/requirement_matching.py`

**Before**: Queries executed sequentially in a loop
```python
for query in queries:
    chunks = await _run_query(query)  # Wait for each one
```

**After**: All queries executed in parallel using `asyncio.gather`
```python
query_tasks = [_run_query(query) for query in queries]
all_query_results = await asyncio.gather(*query_tasks)  # All at once
```

**Speedup**: 3-4x faster for items with multiple queries

### 2. Parallel Item Processing (`_evaluate_all_spec_items`)
**File**: `ui/app_gradio.py`

**Before**: Processed all items sequentially
```python
for obj_dict in objects:
    # ... wait for completion before next item
```

**After**: Process items in parallel batches with configurable concurrency
```python
# Process 5 items at a time (configurable)
for batch_start in range(0, total_items, max_concurrent):
    tasks = [_evaluate_single_item(...) for item in batch]
    batch_results = await asyncio.gather(*tasks)  # Process batch in parallel
```

**Speedup**: Up to 5x faster (with default concurrency of 5)

### 3. Configurable Concurrency
Added a UI control (`max_concurrent`) to balance speed vs API rate limits:
- **Lower values (1-3)**: Slower but safer for strict rate limits
- **Default (5)**: Good balance of speed and reliability
- **Higher values (10-20)**: Faster but may hit rate limits

### Example Timeline (After Optimization)
For 20 items with 3 queries each, with `max_concurrent=5`:
- **Query phase**: 20 items ÷ 5 batches × 3 queries (parallel) × 2 seconds = **8 seconds**
- **LLM phase**: 20 items ÷ 5 batches × 5 seconds (parallel) = **20 seconds**
- **Total: ~28 seconds** - **7.9x faster!**

## Files Modified

1. **`gdd_rag_backbone/gdd/requirement_matching.py`**
   - Optimized `search_code_chunks()` to run queries in parallel

2. **`ui/app_gradio.py`**
   - Refactored `_evaluate_all_spec_items()` to process items in batches
   - Added `_evaluate_single_item()` helper function for parallel execution
   - Updated `evaluate_coverage()` to accept `max_concurrent` parameter
   - Added UI slider for configuring concurrency (1-20, default 5)

## Usage

### In the UI (Gradio)
1. Navigate to the "Code Coverage" tab
2. Adjust the "Max Concurrent Items" slider based on:
   - Your API rate limits
   - How many items need evaluation
   - Desired speed vs reliability trade-off
3. Run the evaluation - it will process items in parallel batches

### Programmatically
```python
# Default: max_concurrent=5
report = await _evaluate_all_spec_items(doc_id, code_index_id, top_k=8)

# Custom concurrency (e.g., for rate-limited APIs)
report = await _evaluate_all_spec_items(doc_id, code_index_id, top_k=8, max_concurrent=3)

# Higher concurrency (faster, but watch rate limits)
report = await _evaluate_all_spec_items(doc_id, code_index_id, top_k=8, max_concurrent=10)
```

## Performance Expectations

| Items | Queries/Item | Before | After (max_concurrent=5) | Speedup |
|-------|-------------|--------|--------------------------|---------|
| 10    | 3           | ~110s  | ~14s                     | 7.9x    |
| 20    | 3           | ~220s  | ~28s                     | 7.9x    |
| 50    | 3           | ~550s  | ~70s                     | 7.9x    |
| 100   | 3           | ~1100s | ~140s                    | 7.9x    |

*Times are approximate and depend on API latency, network speed, and actual item complexity.*

## Notes

- **Rate Limits**: Higher concurrency may hit API rate limits. If you see errors, reduce `max_concurrent`
- **Memory**: Parallel processing uses more memory, but should be negligible for typical workloads
- **Error Handling**: Errors are caught per-item, so one failure doesn't stop the entire evaluation
- **Backward Compatibility**: Default behavior maintains the same API, just faster

## Future Improvements (Optional)

1. **Progress Reporting**: Add real-time progress updates in the UI
2. **Caching**: Cache embedding results for repeated queries
3. **Batch LLM Calls**: If API supports it, batch multiple classification requests
4. **Adaptive Concurrency**: Automatically adjust concurrency based on API response times



