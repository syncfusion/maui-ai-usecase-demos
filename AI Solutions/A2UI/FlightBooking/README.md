# Syncfusion A2UI Showcases

Production-ready demonstration applications showcasing the **A2UI (AI-to-UI) protocol** with Syncfusion components. Each showcase demonstrates end-to-end integration between AI agents (Python) and modern web clients (React/Vite).

## What is A2UI?

**A2UI (AI-to-UI)** is an open protocol that enables AI agents to generate dynamic user interfaces in real-time. Instead of returning text, agents return structured JSON that describes complete UI surfaces with components, data bindings, and event handlers.

### Architecture Overview

```
┌─────────────────┐         HTTP/JSON-RPC          ┌──────────────────┐
│  Web Client     │ ◄──────────────────────────►   │  Python Agent    │
│  (React/Vite)   │     A2UI v0.9 Envelopes       │  (A2A Protocol)  │
│  Port 5173      │                                │  Port 10006      │
└─────────────────┘                                └──────────────────┘
       │                                                     │
       │ Renders                                            │ Generates
       ▼                                                     ▼
┌─────────────────┐                                ┌──────────────────┐
│  Syncfusion     │                                │   AI Models      │
│  Components     │                                │  (GPT/Claude/    │
│  (EJ2 React)    │                                │   Gemini)        │
└─────────────────┘                                └──────────────────┘
```

**Key Components:**
- **Agent (Backend)**: Python-based AI agent using the Syncfusion A2UI Agent ADK
- **Client (Frontend)**: React application with Syncfusion EJ2 components and A2UI renderer
- **Communication**: A2A (Agent-to-Agent) protocol over HTTP/JSON-RPC
- **UI Generation**: A2UI v0.9 JSON envelopes describing complete surfaces

---

## Available Showcases

### 1. Flight Booking (SkyWave Airlines)
**Enterprise flight booking assistant** with 3-stage interactive workflow.

- **Agent**: OpenRouter-compatible (100+ AI models)
- **Client**: React + Vite + TypeScript
- **Features**: Multi-stage forms, data grids, date pickers, passenger management
- **Documentation**: 
  - [Agent Setup Guide](flight-booking/agent/README.md)
  - [Client Setup Guide](flight-booking/client/react/README.md)

---

## Prerequisites

### For All Showcases

#### Agent (Python Backend)
- **Python**: 3.10 or higher (3.11+ recommended)
- **pip**: 21.0 or higher
- **AI Provider API Key**: One of the following:
  - OpenRouter (supports 100+ models) - [Get key](https://openrouter.ai/keys)
  - OpenAI - [Get key](https://platform.openai.com/api-keys)
  - Anthropic Claude - [Get key](https://console.anthropic.com/)
  - Google Gemini - [Get key](https://makersuite.google.com/app/apikey)
  - Azure OpenAI - [Setup guide](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

#### Client (React Frontend)
- **Node.js**: v18 or higher (Latest LTS recommended)
- **npm**: v8 or higher
- **Modern browser**: Chrome, Edge, Firefox, or Safari

---

## Quick Start

> ⚠️ **Two terminals required** — Each showcase requires running both the **agent** (Python) and the **client** (React) simultaneously in separate terminal windows.

### 5-minute setup (flight-booking showcase)

```bash
# 1. Clone the repository
git clone https://github.com/syncfusion/syncfusion-a2ui-showcase.git
cd syncfusion-a2ui-showcase

# 2. Set up the Python agent (Terminal 1)
cd flight-booking/agent
python -m pip install -e .
cd flight_booking_agent
cp .env.example .env
# Open .env and set OPENROUTER_API_KEY (and optionally MODEL_NAME).
# Get a key at https://openrouter.ai/keys (free tier available).
python -m flight_booking_agent.flight_booking_agent
# Agent is now running on http://127.0.0.1:10006

# 3. Set up the React client (Terminal 2)
cd flight-booking/client/react
npm install
npm run dev
# Client is now running on http://localhost:5173

# 4. Open http://localhost:5173 in your browser and start booking.
```

> For troubleshooting, see each sub-project's README:
> [Agent →](flight-booking/agent/README.md)  ·  [Client →](flight-booking/client/react/README.md)
> 
>

### General Workflow

```
Terminal 1: Agent (Python)        Terminal 2: Client (React)
─────────────────────────         ──────────────────────────
1. Install dependencies            4. Install dependencies
2. Configure .env                  5. Configure .env  
3. Run agent                       6. Run client
   → http://127.0.0.1:10006          → http://localhost:5173
   
   Keep running ───────────────────── Keep running
```

### Step-by-Step Instructions

Each showcase has detailed setup guides:

1. **Choose a showcase** (e.g., `flight-booking/`)
2. **Setup the agent** following `<showcase>/agent/README.md`
3. **Setup the client** following `<showcase>/client/react/README.md`
4. **Run both** in separate terminals
5. **Access the application** at `http://localhost:5173`

---

## Common Setup Patterns

### Agent Setup (Python)

All agents follow this pattern:

```bash
# 1. Navigate to agent directory
cd <showcase>/agent

# 2. Install Syncfusion A2UI Agent ADK
pip install "git+https://github.com/syncfusion/syncfusion-a2ui-agent.git"

# 3. Install showcase agent package
pip install -e .

# 4. Configure environment
cd <agent_package_name>
cp .env.example .env
# Edit .env and add your API keys

# 5. Run the agent
python -m <agent_package_name>.<agent_module_name>
# or: python <agent_module_name>.py
```

**Expected Output:**
```
INFO: Uvicorn running on http://127.0.0.1:10006 (Press CTRL+C to quit)
```

### Client Setup (React)

All clients follow this pattern:

```bash
# 1. Navigate to client directory
cd <showcase>/client/react

# 2. Install dependencies
npm install

# 3. Configure environment
# Create or edit .env
VITE_AGENT_URL=http://127.0.0.1:10006/

# 4. Run the client
npm run dev
```

**Expected Output:**
```
VITE v5.4.0  ready in 125 ms
➜  Local:   http://localhost:5173/
```

---

## Environment Configuration

### Agent Environment Variables

Common across all showcases (exact keys depend on provider):

```env
# AI Provider - Choose ONE set below

# OpenRouter (100+ models)
OPENROUTER_API_KEY=your-key-here
MODEL_NAME=google/gemini-3.1-flash-lite

# OpenAI
OPENAI_API_KEY=your-key-here

# Anthropic Claude
ANTHROPIC_API_KEY=your-key-here

# Google Gemini
GOOGLE_API_KEY=your-key-here

# Azure OpenAI
AZURE_API_KEY=your-key-here
AZURE_API_BASE=your-endpoint-here
AZURE_API_VERSION=2025-01-01-preview

# Optional: Syncfusion Platform MCP
Syncfusion_API_Key=your-syncfusion-key-here
SYNCFUSION_REACT_MCP_AVAILABLE=0

# Optional: Custom skills
SKILLS_PATH=./skills

# CORS (use * for local development)
SYNCFUSION_A2A_CORS_ORIGINS=*
```

### Client Environment Variables

```env
# Agent connection (required)
VITE_AGENT_URL=http://127.0.0.1:10006/
```

---

## Troubleshooting

### Common Issues Across Showcases

#### ❌ Agent won't start

**Symptoms**: `ModuleNotFoundError: No module named 'syncfusion_a2ui_agent'`

**Solution**:
```bash
pip install "git+https://github.com/syncfusion/syncfusion-a2ui-agent.git"
```

#### ❌ Client can't connect to agent

**Symptoms**: Network errors, CORS errors, connection refused

**Solutions**:
1. Verify agent is running: Open `http://127.0.0.1:10006/docs`
2. Check `.env` has correct `VITE_AGENT_URL`
3. Ensure CORS is enabled in agent `.env`: `SYNCFUSION_A2A_CORS_ORIGINS=*`
4. Check firewall settings

#### ❌ Port already in use

**Symptoms**: `Address already in use` or `Port 10006 is already allocated`

**Solutions**:
```bash
# Windows
netstat -ano | findstr :10006
taskkill /PID <process_id> /F

# Linux/Mac
lsof -ti:10006 | xargs kill -9
```

#### ❌ Missing environment variables

**Symptoms**: `Missing required environment variables: OPENROUTER_API_KEY`

**Solutions**:
1. Ensure `.env` file exists in the agent package directory
2. Verify API keys are set (no quotes, no spaces)
3. Restart the agent after editing `.env`

---

## Project Structure

```
syncfusion-a2ui-showcase/
├── README.md                          # This file
│
├── flight-booking/                    # Flight booking showcase
│   ├── agent/                         # Python AI agent
│   │   ├── README.md                  # Agent setup guide
│   │   ├── QUICKSTART.md              # Quick reference
│   │   ├── pyproject.toml             # Package config
│   │   └── <agent_package>/
│   │       ├── .env.example           # Environment template
│   │       ├── .env                   # Your config (git-ignored)
│   │       ├── <agent>.py             # Main entry point
│   │       ├── system_prompt.md       # Agent instructions
│   │       ├── custom_providers.py    # AI provider setup
│   │       └── designs/               # A2UI JSON templates
│   │
│   └── client/
│       └── react/                     # React client
│           ├── README.md              # Client setup guide
│           ├── package.json           # Dependencies
│           ├── .env                   # Vite config (git-ignored)
│           ├── index.html             # Vite entrypoint
│           ├── tsconfig.json
│           ├── vite.config.ts
│           ├── public/                # Static assets (favicon)
│           └── src/
│               ├── main.tsx           # React root + EJ2 theme
│               ├── App.tsx            # Main app (chat + A2UI surface)
│               └── App.css            # Layout & component styles
│
└── <future-showcases>/                # More showcases coming soon
```

---

## Technology Stack

### Backend (Agent)
- **Framework**: [Syncfusion A2UI Agent ADK](https://github.com/syncfusion/syncfusion-a2ui-agent)
- **Protocol**: [A2A (Agent-to-Agent)](https://a2a-protocol.org/)
- **Server**: FastAPI + Uvicorn
- **AI Providers**: OpenRouter, OpenAI, Anthropic, Google, Azure, Ollama

### Frontend (Client)
- **Framework**: React 18.3+
- **Build Tool**: Vite 5.4+
- **Language**: TypeScript
- **UI Library**: Syncfusion EJ2 React Components
- **A2UI Runtime**: `@a2ui/web_core` + `@a2ui/react`

---

## Development Workflow

### 1. Design with A2UI Composer

Create your UI designs visually:

1. Open [A2UI Composer](https://a2ui-composer.syncfusion.com/)
2. Describe your desired UI in natural language
3. Export the generated A2UI JSON
4. Place JSON files in agent's `designs/` directory

### 2. Customize the Agent

- **System Prompt**: Edit `system_prompt.md` for agent behavior
- **AI Model**: Change `MODEL_NAME` in `.env`
- **UI Designs**: Update JSON files in `designs/` directory
- **Custom Logic**: Add business logic in agent code

### 3. Customize the Client

- **Styling**: Modify `App.css` or component styles
- **Components**: Add custom React components
- **Themes**: Switch Syncfusion themes in `main.tsx`

---

## Resources

### Documentation
- **A2UI Protocol**: https://a2ui.org
- **A2A Protocol**: https://a2a-protocol.org
- **Syncfusion A2UI ADK**: https://github.com/syncfusion/syncfusion-a2ui-agent
- **Syncfusion React Components**: https://www.syncfusion.com/react-components
- **A2UI Composer**: https://a2ui-composer.syncfusion.com/

### API Keys & Accounts
- **OpenRouter**: https://openrouter.ai/keys (Free tier available)
- **OpenAI**: https://platform.openai.com/api-keys
- **Anthropic**: https://console.anthropic.com/
- **Google Gemini**: https://makersuite.google.com/app/apikey
- **Syncfusion**: https://www.syncfusion.com/account/api-key

### Community
- **GitHub Issues**: Report bugs or request features
- **Syncfusion Forums**: https://www.syncfusion.com/forums
- **Stack Overflow**: Tag with `syncfusion` and `a2ui`

---

## FAQ

**Q: Do I need a Syncfusion license?**  
A: No, the showcases are free and open source. You only need API keys for AI providers.

**Q: Which AI model should I use?**  
A: Start with `google/gemini-3.1-flash-lite` (via OpenRouter) for cost-effective testing. Upgrade to `openai/gpt-4o` for production quality.

**Q: Can I use this in production?**  
A: Yes! These are production-ready templates. Ensure you:
- Use environment-specific API keys
- Enable HTTPS with a reverse proxy
- Set specific CORS origins (not `*`)
- Add proper error handling and logging
- Implement authentication/authorization

**Q: How do I add a new showcase?**  
A: Follow the existing structure:
1. Create `<showcase>/agent/` with Python agent
2. Create `<showcase>/client/react/` with React client
3. Add README files with setup instructions
4. Test the complete workflow

**Q: Can I use other frameworks (Vue, Angular, Svelte)?**  
A: Yes! The agent is framework-agnostic. You'll need to implement the A2UI renderer for your chosen framework using the A2UI specification.
