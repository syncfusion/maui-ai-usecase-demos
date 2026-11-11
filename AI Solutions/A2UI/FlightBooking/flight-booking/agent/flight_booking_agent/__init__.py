"""flight-booking-agent — SkyWave Airlines flight-booking agent.

A thin runtime wrapper around :class:`syncfusion_a2ui_agent.SyncfusionAgent`
that wires up the SkyWave A2UI design catalog at boot and serves the A2A
protocol on ``http://127.0.0.1:10006`` for the bundled React client.

Install (SDK from GitHub + this package from the local checkout)::

    pip install "git+https://github.com/syncfusion/syncfusion-a2ui-agent.git" flight-booking/agent

Run the A2A server::

    flight-booking          # console script registered by the install
"""

from .custom_providers import AzureOpenAIProvider, OpenRouterProvider
from .flight_booking_agent import DESIGNS_DIR, FlightBookingAgent

__all__ = ["DESIGNS_DIR", "FlightBookingAgent", "AzureOpenAIProvider", "OpenRouterProvider"]
