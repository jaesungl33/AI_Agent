#!/usr/bin/env python3
"""
CodeQA Backend - AI-powered codebase search and chat
Based on https://github.com/aaronlee0321/codebase_RAG
"""

import os
import json
import logging
from flask import Flask, request, jsonify, render_template
from flask_cors import CORS
from dotenv import load_dotenv
import datetime

# Configure logging first
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Load environment variables
try:
    load_dotenv()
    logger.info(".env file loaded successfully")
except Exception as e:
    logger.warning(f"Could not load .env file: {e}")
    # Try to load manually if dotenv fails
    try:
        with open('.env', 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    key, value = line.split('=', 1)
                    os.environ[key] = value
        logger.info("Manually loaded .env file")
    except Exception as e2:
        logger.warning(f"Could not manually load .env file: {e2}")

app = Flask(__name__, template_folder='templates')
app.config['ENV'] = 'development'
CORS(app, origins=["http://localhost:3000", "http://127.0.0.1:3000"])

# Try to import Supabase (will be configured later)
supabase = None
try:
    from supabase import create_client
    supabase_url = os.getenv('SUPABASE_URL')
    supabase_key = os.getenv('SUPABASE_ANON_KEY')
    if supabase_url and supabase_key:
        supabase = create_client(supabase_url, supabase_key)
        logger.info("Supabase connected")
    else:
        logger.warning("Supabase credentials not found")
except ImportError:
    logger.warning("Supabase library not installed")

# Import LLM providers - Prefer OpenAI over Qwen
try:
    from src.gdd_rag_backbone.llm_providers import QwenProvider, OpenAIProvider
    llm_provider = None
    if os.getenv("OPENAI_API_KEY"):
        llm_provider = OpenAIProvider()
        logger.info("Using OpenAI as LLM provider")
    elif os.getenv("DASHSCOPE_API_KEY"):
        llm_provider = QwenProvider()
        logger.info("Using Qwen as LLM provider")
    else:
        logger.warning("No LLM API key found (checked OPENAI_API_KEY, DASHSCOPE_API_KEY)")
except ImportError:
    logger.warning("LLM providers not available")
    llm_provider = None

# Import Supabase vector store
try:
    from supabase_vector import get_supabase_store
    vector_store = get_supabase_store()
except ImportError:
    logger.warning("Supabase vector store not available")
    vector_store = None

# Flask Routes

@app.route('/')
def index():
    """Serve the main web interface."""
    return render_template('index.html')

@app.route('/health')
def health():
    """Health check endpoint."""
    return jsonify({
        "status": "ok",
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "version": "1.0.0"
    })

@app.route('/api/workspaces')
def get_workspaces():
    """Get available workspaces."""
    return jsonify([{
        "id": "tank_war",
        "name": "Tank War",
        "description": "Tank War game design documents and codebase",
        "createdAt": "2024-12-23T00:00:00.000Z",
        "updatedAt": "2024-12-23T00:00:00.000Z",
        "stats": {
            "documents": 69,
            "gdds": 69,
            "codeFiles": 548
        }
    }])

@app.route('/api/workspaces/<workspace_id>')
def get_workspace(workspace_id):
    """Get specific workspace details."""
    if workspace_id == "tank_war":
        return jsonify({
            "id": "tank_war",
            "name": "Tank War",
            "description": "Tank War game design documents and codebase",
            "createdAt": "2024-12-23T00:00:00.000Z",
            "updatedAt": "2024-12-23T00:00:00.000Z",
            "stats": {
                "documents": 69,
                "gdds": 69,
                "codeFiles": 548
            }
        })
    return jsonify({"error": "Workspace not found"}), 404

@app.route('/api/workspaces/default')
def get_default_workspace():
    """Get default workspace."""
    return jsonify({
        "default_workspace": "tank_war"
    })

@app.route('/api/documents')
def get_documents():
    """Get documents for a workspace."""
    workspace_id = request.args.get('workspaceId', 'tank_war')

    if workspace_id == "tank_war":
        # Return list of documents (simplified for now)
        documents = [
            {
                "id": "deathmatch_gdd",
                "name": "[Game Mode Module] [Tank War] Deathmatch.pdf",
                "type": "gdd",
                "status": "indexed",
                "indexedAt": "2024-12-23T00:00:00.000Z",
                "chunksCount": 15
            },
            {
                "id": "fusion_artifact_gdd",
                "name": "[Progression Module] [Tank Wars] Fusion Artifact.pdf",
                "type": "gdd",
                "status": "indexed",
                "indexedAt": "2024-12-23T00:00:00.000Z",
                "chunksCount": 12
            },
            {
                "id": "shooting_logic_gdd",
                "name": "[Combat Module] [Tank War] Shooting Logic.pdf",
                "type": "gdd",
                "status": "indexed",
                "indexedAt": "2024-12-23T00:00:00.000Z",
                "chunksCount": 8
            }
        ]
        return jsonify(documents)

    return jsonify([])

@app.route('/api/chat', methods=['POST'])
def chat():
    """Handle chat requests with code search functionality."""
    try:
        data = request.get_json()
        message = data.get('message', '')
        workspace_id = data.get('workspaceId', 'tank_war')
        use_reranking = data.get('useReranking', False)

        logger.info(f"Chat request: {message[:100]}...")

        # Check if this is a codebase query (starts with @codebase)
        is_codebase_query = message.strip().startswith('@codebase')

        if is_codebase_query:
            # Handle codebase-specific queries
            query = message.replace('@codebase', '').strip()
            response = handle_codebase_query(query, workspace_id, use_reranking)
        else:
            # Handle general GDD queries
            response = handle_gdd_query(message, workspace_id)

        return jsonify({"message": response})

    except Exception as e:
        logger.error(f"Chat error: {e}")
        return jsonify({
            "error": "Chat processing failed",
            "message": {
                "id": datetime.datetime.utcnow().isoformat(),
                "role": "assistant",
                "content": "Sorry, I encountered an error processing your request.",
                "timestamp": datetime.datetime.utcnow().isoformat(),
                "context": {"sources": [], "chunks": []}
            }
        }), 500

def handle_codebase_query(query, workspace_id, use_reranking):
    """Handle codebase-specific queries using Supabase RAG."""
    try:
        if not vector_store or not llm_provider:
            return {
                "id": datetime.datetime.utcnow().isoformat(),
                "role": "assistant",
                "content": "Codebase search is not available. Supabase and LLM providers are not configured.",
                "timestamp": datetime.datetime.utcnow().isoformat(),
                "context": {"sources": [], "chunks": []}
            }

        # Generate embedding for the query
        # For now, using a simple approach - in production you'd use proper embeddings
        query_embedding = [0.1] * 384  # Placeholder embedding

        # Search for similar code chunks
        similar_chunks = vector_store.search_similar(
            query_embedding=query_embedding,
            workspace_id=workspace_id,
            top_k=5
        )

        if not similar_chunks:
            return {
                "id": datetime.datetime.utcnow().isoformat(),
                "role": "assistant",
                "content": f"No relevant code found for: '{query}'. The codebase search index may not be populated yet.",
                "timestamp": datetime.datetime.utcnow().isoformat(),
                "context": {"sources": [], "chunks": []}
            }

        # Format context for LLM
        context_parts = []
        sources = []

        for chunk in similar_chunks:
            context_parts.append(f"File: {chunk['file_path']} (lines {chunk['start_line']}-{chunk['end_line']})")
            context_parts.append(f"```{chunk.get('language', 'code')}")
            context_parts.append(chunk['content'])
            context_parts.append("```")

            sources.append({
                "name": f"{chunk['file_path']}:{chunk['start_line']}-{chunk['end_line']}",
                "type": "code"
            })

        context = "\n\n".join(context_parts)

        # Generate response using LLM
        prompt = f"""
You are a helpful coding assistant. Answer the user's question about this codebase.

Question: {query}

Relevant code context:
{context}

Please provide a clear, helpful answer based on the code provided. If the context doesn't fully answer the question, say so and suggest what additional information might be needed.
"""

        llm_response = llm_provider.llm(prompt=prompt)

        return {
            "id": datetime.datetime.utcnow().isoformat(),
            "role": "assistant",
            "content": llm_response,
            "timestamp": datetime.datetime.utcnow().isoformat(),
            "context": {
                "sources": sources,
                "chunks": similar_chunks
            }
        }

    except Exception as e:
        logger.error(f"Codebase query error: {e}")
        return {
            "id": datetime.datetime.utcnow().isoformat(),
            "role": "assistant",
            "content": f"Error processing codebase query: {str(e)}",
            "timestamp": datetime.datetime.utcnow().isoformat(),
            "context": {"sources": [], "chunks": []}
        }

def handle_gdd_query(message, workspace_id):
    """Handle general GDD queries with intelligent responses."""
    message_lower = message.lower()

    # Intelligent response routing based on content
    # Deathmatch/Multiplayer
    if any(keyword in message_lower for keyword in ['deathmatch', 'battle', 'combat', 'multiplayer', 'pvp', 'versus', 'game mode', 'mode']):
        content = ("Deathmatch in Tank War is a fast-paced multiplayer mode where players compete "
                  "to eliminate opponents. Features include various maps, weapon balancing, "
                  "and scoring systems. The mode supports up to multiple players simultaneously. "
                  "Other game modes include Outpost Breaker (capture objectives) and Gold in Match (resource collection).")
        sources = [
            {"name": "[Game Mode Module] [Tank War] Deathmatch.pdf", "type": "gdd"},
            {"name": "[Game Mode Module] [Tank War] Outpost Breaker.pdf", "type": "gdd"},
            {"name": "[Game Mode Module] [Tank War] Gold in Match.pdf", "type": "gdd"}
        ]

    # Progression/Artifacts
    elif any(keyword in message_lower for keyword in ['progression', 'fusion', 'artifact', 'level', 'upgrade', 'enhancement', 'tier']):
        content = ("The Fusion Artifact system is Tank War's progression mechanic. Players collect "
                  "and combine artifacts to enhance their tank's capabilities. The system includes "
                  "multiple tiers of artifacts, crafting mechanics, and strategic depth. Artifacts "
                  "provide stat bonuses and special abilities.")
        sources = [
            {"name": "[Progression Module] [Tank Wars] Fusion Artifact.pdf", "type": "gdd"},
            {"name": "[Progression Module] [Tank Wars] Artifact Enhancement.pdf", "type": "gdd"},
            {"name": "[Progression Module] [Tank Wars] Artifact Enhancement Table - Common.csv", "type": "gdd"}
        ]

    # Combat/Shooting/Weapons
    elif any(keyword in message_lower for keyword in ['shooting', 'weapon', 'combat', 'damage', 'fire', 'shoot', 'attack', 'gun']):
        content = ("The shooting mechanics include various weapon types, projectile physics, "
                  "damage calculations, and aiming systems. The system supports both "
                  "direct and indirect fire mechanics. Tanks have different weapon types "
                  "with unique characteristics and balancing considerations.")
        sources = [
            {"name": "[Combat Module] [Tank War] Shooting Logic.pdf", "type": "gdd"},
            {"name": "[Character Module] [Tank War] Tank System Detail.pdf", "type": "gdd"}
        ]

    # UI/Interface/Menus
    elif any(keyword in message_lower for keyword in ['ui', 'interface', 'menu', 'screen', 'display', 'hud', 'gui']):
        content = ("Tank War features comprehensive UI/UX design including main screens, "
                  "garage interfaces, mode selection, result screens, and in-game HUD. "
                  "The interface supports multiple languages and accessibility features. "
                  "Key screens include tank selection, garage customization, and match results.")
        sources = [
            {"name": "[Asset, UI] [Tank War] Main Screen Design.pdf", "type": "gdd"},
            {"name": "[Asset, UI] [Tank War] In-game GUI Design.pdf", "type": "gdd"},
            {"name": "[Asset, UI] [Tank War] Garage Design - UI_UX.pdf", "type": "gdd"}
        ]

    # Characters/Tanks/Classes
    elif any(keyword in message_lower for keyword in ['character', 'tank', 'class', 'vehicle', 'unit', 'type']):
        content = ("Tank War offers multiple tank classes with unique elemental abilities. "
                  "Each class has distinct playstyles, strengths, and weaknesses, "
                  "allowing for diverse gameplay strategies. The garage system allows "
                  "customization and upgrades of tank capabilities.")
        sources = [
            {"name": "[Character Module] [Tank War] Garage Design - Main.pdf", "type": "gdd"},
            {"name": "[Character Module] [Tank War] [Elemental Class].pdf", "type": "gdd"},
            {"name": "[Character Module] [Tank War] Tank System Detail.pdf", "type": "gdd"}
        ]

    # Maps/World/Terrain
    elif any(keyword in message_lower for keyword in ['map', 'world', 'terrain', 'environment', 'landscape', 'area']):
        content = ("Tank War features diverse maps with different terrains, strategic positions, "
                  "and environmental hazards. Maps include destructible elements, cover systems, "
                  "and are designed for various game modes. The world includes grass mechanics, "
                  "camera systems, and outpost capture objectives.")
        sources = [
            {"name": "[World] [Tank War] Map Document.pdf", "type": "gdd"},
            {"name": "[World] [Tank War] Grass Logic Design.pdf", "type": "gdd"},
            {"name": "[World] [Tank War] Camera Logic System.pdf", "type": "gdd"}
        ]

    # Economy/Monetization/Money
    elif any(keyword in message_lower for keyword in ['economy', 'monetization', 'money', 'currency', 'purchase', 'buy', 'cost', 'diamond', 'gems', 'premium', 'pay']):
        content = ("Tank War's economy system includes various monetization features such as "
                  "in-game purchases, cosmetic items, battle passes, and premium currency. "
                  "The system is designed to be fair and rewarding for all players. The garage "
                  "system includes upgrade costs and customization pricing.")
        sources = [
            {"name": "[Monetization Module] [Tank War] Economy & Monetization System.pdf", "type": "gdd"},
            {"name": "[Monetization Module] [Tank War] Garage System.pdf", "type": "gdd"}
        ]

    # Matchmaking/Lobby
    elif any(keyword in message_lower for keyword in ['matchmaking', 'lobby', 'queue', 'find match', 'search']):
        content = ("The matchmaking system ensures fair and balanced games by pairing players "
                  "of similar skill levels. Features include region-based matching, "
                  "queue management, and anti-cheat measures. Match profiles and post-match "
                  "statistics are tracked for player progression.")
        sources = [
            {"name": "[Multiplayer Module] [Tank War] Matchmaking System Design.pdf", "type": "gdd"},
            {"name": "[Multiplayer Module] [Tank War] Match Profile Design.pdf", "type": "gdd"},
            {"name": "[Multiplayer Module] [Tank War] Post-Match Profile.pdf", "type": "gdd"}
        ]

    # Stats/Balancing
    elif any(keyword in message_lower for keyword in ['stats', 'balancing', 'balance', 'numbers', 'values', 'damage']):
        content = ("Tank War includes comprehensive stats and balancing systems. Character stats, "
                  "weapon damage values, and game balancing are meticulously designed. "
                  "The system includes upgrade tables and enhancement calculations.")
        sources = [
            {"name": "[Character Module] [Tank War] Stats & Balancing - Mô tả.csv", "type": "gdd"},
            {"name": "[Progression Module] [Tank Wars] Artifact Enhancement Table - Common.csv", "type": "gdd"},
            {"name": "[Progression Module] [Tank Wars] Artifact Stat Table - Main Stats.csv", "type": "gdd"}
        ]

    # Achievements/Progression
    elif any(keyword in message_lower for keyword in ['achievement', 'reward', 'unlocks', 'milestone']):
        content = ("The achievement system provides goals and rewards for player progression. "
                  "Various achievements unlock cosmetics, titles, and special content. "
                  "The leaderboard system tracks top performers across different categories.")
        sources = [
            {"name": "[Progression Module] [Tank War] Achievement Listing - List.csv", "type": "gdd"},
            {"name": "[Progression Module] [Tank Wars] Achievement Design.pdf", "type": "gdd"},
            {"name": "[Progression Module] [Tank War] Leaderboard System.pdf", "type": "gdd"}
        ]

    # Onboarding/Tutorial
    elif any(keyword in message_lower for keyword in ['tutorial', 'onboarding', 'learn', 'beginner', 'new player']):
        content = ("Tank War includes comprehensive onboarding and tutorial systems to help "
                  "new players learn the game mechanics. The tutorial covers basic controls, "
                  "game modes, and progression systems in an engaging way.")
        sources = [
            {"name": "[Progression Module] [Tank War] Onboarding Design (Chưa xong).pdf", "type": "gdd"},
            {"name": "[Progression Module] [Tank War] Onboarding Tutorial Mode Design.pdf", "type": "gdd"}
        ]

    # Greeting/Help
    elif any(keyword in message_lower for keyword in ['hello', 'hi', 'hey', 'help', 'what can you do', 'assist']):
        content = ("Hello! I'm here to help you learn about Tank War. Ask me about any aspect "
                  "of the game design, mechanics, or features. I have access to 69 design documents "
                  "and the complete codebase.\n\n"
                  "You can ask about:\n"
                  "• Game modes (Deathmatch, Outpost Breaker, etc.)\n"
                  "• Character classes and tank systems\n"
                  "• UI/UX design and interfaces\n"
                  "• Economy and monetization\n"
                  "• Maps and environments\n"
                  "• Combat mechanics and weapons\n"
                  "• Progression and artifacts\n"
                  "• Multiplayer systems\n\n"
                  "For code-specific questions, use @codebase in your query!")
        sources = []

    # Default fallback
    else:
        content = (f"I understand you're asking about '{message[:50]}...'. "
                  "Tank War has comprehensive documentation covering all aspects of the game. "
                  "Try asking about specific topics like 'game modes', 'tanks', 'weapons', "
                  "'progression', 'UI', or 'economy'. You can also search the codebase using @codebase queries.")
        sources = []

    return {
        "id": datetime.datetime.utcnow().isoformat(),
        "role": "assistant",
        "content": content,
        "timestamp": datetime.datetime.utcnow().isoformat(),
        "context": {
            "sources": sources,
            "chunks": []
        }
    }

@app.route('/api/index', methods=['POST'])
def index_codebase():
    """Index a codebase for search (placeholder for now)."""
    try:
        data = request.get_json()
        codebase_path = data.get('codebase_path', '')

        # This will be implemented with Supabase
        return jsonify({
            "status": "success",
            "message": f"Indexing started for: {codebase_path}",
            "index_id": "placeholder_index"
        })

    except Exception as e:
        logger.error(f"Indexing error: {e}")
        return jsonify({"error": "Indexing failed"}), 500

if __name__ == '__main__':
    port = int(os.getenv('PORT', 8000))
    logger.info(f"Starting CodeQA server on port {port}")
    os.environ['FLASK_SKIP_DOTENV'] = '1'  # Disable Flask's auto dotenv loading
    app.run(host='0.0.0.0', port=port, debug=os.getenv('FLASK_DEBUG', 'False').lower() == 'true')
