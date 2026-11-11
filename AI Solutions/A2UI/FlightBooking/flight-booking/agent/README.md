# SkyWave Airlines Flight Booking Agent

A production-ready AI agent for flight booking built on the **Syncfusion A2UI Agent ADK**. This agent serves A2UI v0.9 envelopes over the A2A (Agent-to-Agent) protocol for a 3-stage flight booking workflow.

## Overview

The **SkyWave Flight Booking Agent** is a specialized A2UI agent that:
- Serves a 3-stage interactive booking flow (Search → Results → Confirmation)
- Uses pre-authored A2UI design templates for consistent UI generation
- Connects to OpenRouter for AI model access (supports 100+ models including GPT-4, Claude, Gemini)
- Exposes an A2A HTTP/JSON-RPC server on `http://127.0.0.1:10006`
- Integrates with the React client for real-time UI rendering

### Architecture
```
┌─────────────────┐         HTTP/JSON-RPC          ┌──────────────────┐
│  React Client   │ ◄──────────────────────────►   │  Python Agent    │
│  (Port 5173)    │     A2UI v0.9 Envelopes       │  (Port 10006)    │
└─────────────────┘                                └──────────────────┘
                                                            │
                                                            │ API Calls
                                                            ▼
                                                   ┌──────────────────┐
                                                   │   OpenRouter     │
                                                   │ (Gemini/GPT-4)   │
                                                   └──────────────────┘
```

---

## Prerequisites

### System Requirements
- **Python**: 3.10 or higher (3.11+ recommended)
- **pip**: 21.0 or higher
- **OS**: Windows, macOS, or Linux

### Required API Keys
1. **OpenRouter API Key** ([Get one here](https://openrouter.ai/keys))
   - Free tier available
   - Provides access to 100+ AI models (GPT-4, Claude, Gemini, etc.)
   - Required for agent AI processing

2. **Syncfusion API Key** (Optional - only if using Syncfusion Platform MCP)
   - Get from [Syncfusion Account Dashboard](https://www.syncfusion.com/account/api-key)
   - Currently disabled in this setup (`SYNCFUSION_REACT_MCP_AVAILABLE=0`)
   - Not a product license - it's an account-level API key

### Verify Python Installation
```bash
python --version
# Should show Python 3.10.0 or higher
```

---

## Installation

### Step 1: Navigate to Agent Directory
```bash
cd flight-booking\agent
```

### Step 2: Install the Flight Booking Agent Package
```bash
python -m pip install -e .
```

This installs:
- `syncfusion-a2ui-agent` - The core ADK framework
- `a2a-python` - A2A protocol implementation
- Provider libraries (OpenAI, Anthropic, Google AI, etc.)
- All required dependencies

**Installation time**: ~30-60 seconds depending on your connection

### Step 3: Verify Installation
```bash
python -c "import syncfusion_a2ui_agent; print('ADK installed successfully')"
python -c "import flight_booking_agent; print('Flight Booking Agent installed successfully')"
```

Both commands should complete without errors.

---

## Configuration

### Step 1: Create Environment File
The agent requires environment variables for API keys and configuration. Create a `.env` file from the template:

```bash
cd flight_booking_agent
cp .env.example .env
```

### Step 2: Configure API Keys
Edit `.env` and add your API keys:

```env
# OpenRouter API Key (Required)
# Get from: https://openrouter.ai/keys
OPENROUTER_API_KEY=your-openrouter-api-key-here

# AI Model Selection
# Format: provider/model (e.g., openai/gpt-4o, anthropic/claude-3-5-sonnet)
# See https://openrouter.io/docs/models for available models
MODEL_NAME=google/gemini-3.1-flash-lite

# Syncfusion Platform Key (Optional - only if using Platform MCP)
# Get from: https://www.syncfusion.com/account/api-key
Syncfusion_API_Key=your-syncfusion-api-key-here

# Platform MCP Toggle (0 = disabled, 1 = enabled)
SYNCFUSION_REACT_MCP_AVAILABLE=0

# Skills Path (Optional - for custom agent skills)
SKILLS_PATH=

# CORS Origins (for web client access)
SYNCFUSION_A2A_CORS_ORIGINS=*
```

### Step 3: Verify Configuration Files

Ensure these files exist:
```bash
flight_booking_agent/
├── .env                                 # Your API keys (git-ignored)
├── .env.example                         # Template file
├── flight_booking_agent.py              # Main agent script
├── system_prompt.md                     # Agent instructions
├── custom_providers.py                  # OpenRouter provider
└── designs/
    ├── stage1-flight-search.json        # Search UI design
    ├── stage2-flight-results.json       # Results UI design
    └── stage3-booking-confirmation.json # Confirmation UI design
```

**Important**: The `designs/` directory must contain all 3 stage JSON files. Missing designs will cause the agent to fail at startup.

---

## Running the Agent

### Method 1: Using Python Module (Recommended)
From the `agent` directory:

```bash
python -m flight_booking_agent.flight_booking_agent
```

### Method 2: Using Direct Script
From the `agent/flight_booking_agent` directory:

```bash
cd flight_booking_agent
python flight_booking_agent.py
```

### Expected Output
```
INFO: Starting FlightBookingAgent...
INFO: Initializing OpenRouter provider with model: google/gemini-3.1-flash-lite, base_url: https://openrouter.ai/api/v1
>>> FlightBookingAgent.__init__() starting...
>>> extend_system_prompt() called
INFO: extend_system_prompt() called
>>> Loaded system_prompt.md: 13393 chars
INFO: Loaded system_prompt.md: 13393 chars from D:\A2UI\...\system_prompt.md
>>> FlightBookingAgent.__init__() complete. System prompt size: 13393 chars
WARNING: SKILLS_PATH not set - skill routing disabled. Add SKILLS_PATH=<path> to your .env to enable it.
INFO: A2UI streaming enabled – message/stream requests will fan out envelope ops per-event over SSE.
INFO:     Started server process [20944]
INFO:     Waiting for application startup.
INFO:     Application startup complete.
INFO:     Uvicorn running on http://127.0.0.1:10006 (Press CTRL+C to quit)
```

### Verification
The agent is ready when you see:
```
INFO:     Uvicorn running on http://127.0.0.1:10006 (Press CTRL+C to quit)
```

You can verify the server is running by opening a browser to:
```
http://127.0.0.1:10006/docs
```
This shows the FastAPI auto-generated API documentation.

---

## Testing the Agent

### Option 1: Using the React Client (Recommended)
The full UI experience requires the React client:

1. **Keep the agent running** (Terminal 1)
2. Open a **new terminal** (Terminal 2)
3. Navigate to client:
   ```bash
   # from the repository root
   cd flight-booking/client/react
   ```
4. Start the client:
   ```bash
   npm install  # First time only
   npm run dev
   ```
5. Open browser to: `http://localhost:5173`

See [`../client/react/README.md`](../client/react/README.md) for full client setup.

### Option 2: Using curl (Quick Test)
Test the agent endpoint directly:

```bash
curl -X POST http://127.0.0.1:10006/message/send \
  -H "Content-Type: application/json" \
  -d "{\"prompt\": \"I want to search for flights\"}"
```

You should receive a JSON response with an A2UI envelope.

### Option 3: Using Python Script
Create a test script `test_agent.py`:

```python
import requests

response = requests.post(
    "http://127.0.0.1:10006/message/send",
    json={"prompt": "Show me flights from New York to London"}
)

print("Status:", response.status_code)
print("Response:", response.json())
```

Run with: `python test_agent.py`

---

## Stopping the Agent

Press `Ctrl+C` in the terminal where the agent is running.

Expected output:
```
^CINFO:     Shutting down
INFO:     Finished server shutdown.
```

---

## Troubleshooting

### Issue 1: `ModuleNotFoundError: No module named 'syncfusion_a2ui_agent'`

**Cause**: The ADK is not installed.

**Solution**:
```bash
pip install "git+https://github.com/syncfusion/syncfusion-a2ui-agent.git"
```

### Issue 2: `FileNotFoundError: .env file not found`

**Cause**: Missing `.env` file in the `flight_booking_agent/` directory.

**Solution**:
```bash
cd flight_booking_agent
cp .env.example .env
# Edit .env and add your API keys
```

### Issue 3: `Missing required environment variables: OPENROUTER_API_KEY, MODEL_NAME`

**Cause**: `.env` file exists but API keys are not set.

**Solution**:
1. Open `flight_booking_agent/.env`
2. Replace placeholder values with your actual API keys
3. Save the file and restart the agent

### Issue 4: `FileNotFoundError: Designs directory not found`

**Cause**: Missing or misplaced `designs/` directory.

**Solution**:
Verify the directory structure:
```bash
flight_booking_agent/
└── designs/
    ├── stage1-flight-search.json
    ├── stage2-flight-results.json
    └── stage3-booking-confirmation.json
```

All 3 JSON files must be present.

### Issue 5: Port 10006 Already in Use

**Cause**: Another process is using port 10006, or a previous agent instance is still running.

**Solution**:
```bash
# On Windows - find and kill the process
netstat -ano | findstr :10006
taskkill /PID <process_id> /F

# On Linux/Mac
lsof -ti:10006 | xargs kill -9
```

### Issue 6: Agent Starts but Client Can't Connect

**Cause**: CORS configuration or network firewall.

**Solution**:
1. Check `.env` has: `SYNCFUSION_A2A_CORS_ORIGINS=*`
2. Verify agent is running: `http://127.0.0.1:10006/docs`
3. Check firewall settings allow local connections

### Issue 8: OpenRouter API Errors

**Cause**: Invalid API key, rate limit, or model not available.

**Solution**:
1. Verify API key at [OpenRouter Keys](https://openrouter.ai/keys)
2. Check model name format: `provider/model` (e.g., `google/gemini-3.1-flash-lite`)
3. See available models: [OpenRouter Models](https://openrouter.io/docs/models)
4. Try a different model if current one is unavailable

---

## Project Structure

```
flight-booking/agent/
├── README.md                          # This file
├── pyproject.toml                     # Package configuration
└── flight_booking_agent/
    ├── .env                           # Your API keys (git-ignored)
    ├── .env.example                   # Environment template
    ├── __init__.py                    # Package marker
    ├── flight_booking_agent.py        # Main agent entry point
    ├── system_prompt.md               # Agent instructions (13KB)
    ├── custom_providers.py            # OpenRouter provider implementation
    └── designs/                       # A2UI design templates
        ├── stage1-flight-search.json        # Search form UI
        ├── stage2-flight-results.json       # Results grid UI
        └── stage3-booking-confirmation.json # Confirmation summary UI
```

---

## Configuration Reference

### Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `OPENROUTER_API_KEY` | ✅ Yes | - | Your OpenRouter API key |
| `MODEL_NAME` | ✅ Yes | - | AI model in `provider/model` format |
| `Syncfusion_API_Key` | ⚠️ Optional | - | Only needed if `SYNCFUSION_REACT_MCP_AVAILABLE=1` |
| `SYNCFUSION_REACT_MCP_AVAILABLE` | No | `0` | `1` to enable Platform MCP, `0` to disable |
| `SKILLS_PATH` | No | - | Path to custom agent skills directory |
| `SYNCFUSION_A2A_CORS_ORIGINS` | No | `*` | CORS allowed origins (use `*` for local dev) |

### Supported AI Models (via OpenRouter)

The agent works with any model available on OpenRouter. Popular choices:

| Provider | Model | MODEL_NAME Value | Use Case |
|----------|-------|------------------|----------|
| Google | Gemini 3.1 Flash Lite | `google/gemini-3.1-flash-lite` | Fast, cost-effective (current default) |
| OpenAI | GPT-4o | `openai/gpt-4o` | High quality, best for complex queries |
| Anthropic | Claude 3.5 Sonnet | `anthropic/claude-3-5-sonnet` | Balanced quality and speed |
| Meta | Llama 3.3 70B | `meta-llama/llama-3.3-70b-instruct` | Open source, free tier |

See the full list at: https://openrouter.io/docs/models

---

## Next Steps

### 1. Start the React Client
After the agent is running, start the React client to get the full UI:
```bash
cd ../client/react
npm install
npm run dev
```
Then open: `http://localhost:5173`

### 2. Customize the Agent

**Change AI Model**: Edit `.env` and change `MODEL_NAME`:
```env
MODEL_NAME=openai/gpt-4o
```

**Add Custom Skills**: Create `SKILL.md` files and set `SKILLS_PATH`:
```env
SKILLS_PATH=./skills
```

**Modify System Prompt**: Edit `flight_booking_agent/system_prompt.md`

**Update UI Designs**: Edit JSON files in `flight_booking_agent/designs/`

### 3. Deploy to Production

For production deployment:
1. Set specific CORS origins (not `*`)
2. Use environment-specific API keys
3. Enable HTTPS with a reverse proxy (nginx, Traefik)
4. Set up monitoring and logging
5. Use a process manager (systemd, PM2, Docker)

---

## Support & Resources

- **Syncfusion A2UI ADK**: https://github.com/syncfusion/syncfusion-a2ui-agent
- **A2A Protocol**: https://a2a-protocol.org/
- **OpenRouter**: https://openrouter.ai/docs
- **Syncfusion Components**: https://www.syncfusion.com/react-components

---

## FAQ

**Q: Do I need a Syncfusion license?**  
A: No, the agent itself is free and open source. You only need a Syncfusion API key if enabling the Platform MCP (`SYNCFUSION_REACT_MCP_AVAILABLE=1`), which is currently disabled.

**Q: Which AI model should I use?**  
A: Start with `google/gemini-3.1-flash-lite` (current default) for cost-effective testing. Upgrade to `openai/gpt-4o` for production quality.

**Q: Can I use this without OpenRouter?**  
A: Yes, but you'd need to modify `custom_providers.py` to add support for other providers. The ADK supports Azure OpenAI, OpenAI, Anthropic Claude, Google Gemini, and Ollama out of the box.

**Q: How do I change the port from 10006?**  
A: Edit `flight_booking_agent.py` line 167: `agent.serve(host="127.0.0.1", port=YOUR_PORT)`

**Q: The agent works but the UI doesn't show flights?**  
A: The agent generates UI designs - actual flight data would come from a real booking API integration, which is not included in this demo.
