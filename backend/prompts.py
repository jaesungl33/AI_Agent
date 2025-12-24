"""
Prompt templates for code QA system.
Based on code_qa implementation.
"""

from typing import List, Dict, Any
import jinja2


class PromptManager:
    """Manage prompt templates for code QA."""

    def __init__(self):
        self.env = jinja2.Environment()

    def get_hyde_prompt(self, query: str) -> str:
        """Generate hypothetical document for query expansion."""
        template = """
You are an expert software engineer. Given a query about code, generate a hypothetical code snippet that would answer the query.

Query: {{ query }}

Generate a code example that would be found in a codebase that answers this query. Include relevant function names, class names, and code structure.

Hypothetical code snippet:
"""
        prompt_template = self.env.from_string(template)
        return prompt_template.render(query=query)

    def get_context_filtering_prompt(self, query: str, contexts: List[str]) -> str:
        """Generate prompt for filtering relevant contexts."""
        contexts_text = "\n\n".join(f"[Context {i+1}]\n{ctx}" for i, ctx in enumerate(contexts))

        template = """
You are an expert software engineer. Given a query about code and multiple context snippets, identify which contexts are most relevant to answering the query.

Query: {{ query }}

Contexts:
{{ contexts }}

Return only the indices (starting from 1) of the most relevant contexts, separated by commas. Return at most 5 context indices.
"""
        prompt_template = self.env.from_string(template)
        return prompt_template.render(query=query, contexts=contexts_text)

    def get_answer_generation_prompt(self, query: str, contexts: List[Dict[str, Any]]) -> str:
        """Generate prompt for answering query with contexts."""
        context_parts = []
        for i, ctx in enumerate(contexts):
            context_parts.append(f"""
[Context {i+1}]
File: {ctx.get('file_path', 'Unknown')}
Symbol: {ctx.get('full_name', 'Unknown')}
Lines: {ctx.get('line_start', 0)}-{ctx.get('line_end', 0)}
Code:
{ctx.get('content', '')}
""")

        contexts_text = "\n".join(context_parts)

        template = """
You are an expert software engineer helping with code analysis. Answer the user's question using the provided code contexts.

Query: {{ query }}

Relevant Code Contexts:
{{ contexts }}

Instructions:
1. Answer based only on the provided code contexts
2. Cite specific files, functions, and line numbers
3. If the contexts don't contain enough information, say so clearly
4. Be concise but comprehensive
5. Include code snippets when relevant

Answer:
"""
        prompt_template = self.env.from_string(template)
        return prompt_template.render(query=query, contexts=contexts_text)


# Global prompt manager
prompt_manager = PromptManager()


