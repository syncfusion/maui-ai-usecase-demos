#!/usr/bin/env python3
"""Offline stand-in for the SkyWave agent (no LLM / API key needed) for trying the MAUI client.

Serves A2A JSON-RPC `message/send` on http://127.0.0.1:10006 and answers with the bundled
`designs_maui` envelopes, choosing the stage from the request text / action name.

    python3 harness/mock_agent.py [port]
"""
import json, os, sys, uuid
from http.server import BaseHTTPRequestHandler, HTTPServer

DESIGNS = os.path.join(os.path.dirname(__file__), "..", "..", "..", "agent", "flight_booking_agent", "designs_maui")
FILES = {1: "stage1-flight-search.json", 2: "stage2-flight-results.json", 3: "stage3-booking-confirmation.json"}
SAY = {1: "Showing the flight search form. Enter your route and details, then press Search Flights.",
       2: "Here are the available flights. Tap a row, then press Book now.",
       3: "Your booking is confirmed. Your e-ticket has been issued."}


def pick_stage(text: str) -> int:
    t = text.lower()
    if "selectflightandproceed" in t or "book now" in t or "confirmation" in t or "booking and confirmation" in t:
        return 3
    if "searchflights" in t or "search flights" in t or "results" in t:
        return 2
    return 1


def envelope(stage: int):
    with open(os.path.join(DESIGNS, FILES[stage]), encoding="utf-8") as f:
        msgs = json.load(f)
    msgs.append({"version": "v0.9", "updateDataModel": {"surfaceId": "workspace", "path": "/_explanation", "value": SAY[stage]}})
    return msgs


class Handler(BaseHTTPRequestHandler):
    def _send(self, code, obj):
        body = json.dumps(obj).encode()
        self.send_response(code)
        self.send_header("Content-Type", "application/json")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def _body(self) -> bytes:
        if self.headers.get("Transfer-Encoding", "").lower() == "chunked":
            out = b""
            while True:
                n = int(self.rfile.readline().strip() or b"0", 16)
                if n == 0:
                    self.rfile.readline()
                    return out
                out += self.rfile.read(n)
                self.rfile.readline()
        return self.rfile.read(int(self.headers.get("Content-Length", 0)))

    def do_OPTIONS(self):
        self.send_response(204)
        for k, v in (("Access-Control-Allow-Origin", "*"), ("Access-Control-Allow-Headers", "*"), ("Access-Control-Allow-Methods", "POST, OPTIONS")):
            self.send_header(k, v)
        self.end_headers()

    def do_POST(self):
        req = json.loads(self._body() or b"{}")
        msg = (req.get("params") or {}).get("message") or {}
        text = " ".join(p.get("text", "") for p in msg.get("parts", []))
        stage = pick_stage(text)
        print(f"[mock-agent] stage {stage} <- {text[:90]!r}", flush=True)
        self._send(200, {"jsonrpc": "2.0", "id": req.get("id"), "result": {
            "contextId": msg.get("contextId") or f"ctx-{uuid.uuid4().hex[:8]}",
            "artifacts": [{"parts": [{"data": {"a2uiEnvelope": envelope(stage)}}]}],
        }})

    def log_message(self, *a):
        pass


if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 10006
    print(f"[mock-agent] listening on http://127.0.0.1:{port}", flush=True)
    HTTPServer(("127.0.0.1", port), Handler).serve_forever()
