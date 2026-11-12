# SkyBook — .NET 10 MAUI client (Syncfusion.Maui.A2UI)

MAUI port of `../react`. Same agent, same A2A endpoint, same three-stage flow; booking screens are rendered by
`Syncfusion.Maui.A2UI` from A2UI v0.9 envelopes.

* Harness (start here): [`harness/INTENT.md`](harness/INTENT.md) · [`PLAN.md`](harness/PLAN.md) ·
  [`TASKS.md`](harness/TASKS.md) · [`ARCHITECTURE.md`](harness/ARCHITECTURE.md)
* Verify: `harness/verify.sh` (tests + design JSON + Android build)

## Running
**No Python needed.** By default the app runs its own agent in C# (`SkyBook.Core/LocalAgent.cs`): same stage gate as the
Python agent, same `designs_maui` pages. Search Flights / View Results / Review Booking and the form buttons work with nothing else running.
To let it understand free text ("fill details on your own and find the flights"), give it Azure OpenAI, either by environment variables
`AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_DEPLOYMENT` (and optional `AZURE_OPENAI_API_VERSION`), or by creating
`src/SkyBook.Maui/Resources/Raw/azure-openai.local.json` (git-ignored):
`{ "endpoint": "https://<resource>.openai.azure.com/", "apiKey": "<key>", "deployment": "<deployment name>" }`.
Set `SKYBOOK_AGENT_URL` only if you want to use the Python agent instead. The steps below are for that optional setup.

1. Start the agent with the MAUI designs: set `FLIGHT_BOOKING_CLIENT=maui` (plus the usual `.env` — see `agent/README.md`), then `flight-booking`.
2. Install the MAUI workload (`dotnet workload install maui`) and run `src/SkyBook.Maui` for your platform.
   No API key handy? Run the offline stand-in instead: `python3 harness/mock_agent.py` (serves the bundled `designs_maui` on the same port).
   If neither is running the app shows "I can't reach the SkyBook agent at <url>" in the chat.
3. Agent URL defaults to `http://localhost:10006` (`http://10.0.2.2:10006` on Android emulator); override with `SKYBOOK_AGENT_URL`.
4. Syncfusion license: set `AppConfig.SyncfusionLicenseKey` (do not commit it).
