"""Custom AI providers for the flight booking agent.

This module contains the OpenRouterProvider implementation that extends
the ADK's built-in providers to enable access to 100+ models via OpenRouter.
"""

from __future__ import annotations

import logging
import os
from collections.abc import AsyncIterator, Iterable
from typing import Any

try:
    from openai import AsyncOpenAI, AsyncAzureOpenAI
except ImportError:
    AsyncOpenAI = None  # type: ignore[assignment, misc]
    AsyncAzureOpenAI = None  # type: ignore[assignment, misc]

from syncfusion_a2ui_agent.providers import (
    AIProvider,
    ModelResponse,
    ProviderError,
)
from syncfusion_a2ui_agent.providers.base import StreamEvent

log = logging.getLogger(__name__)




class AzureOpenAIProvider(AIProvider):
    name = "azure-openai"

    def __init__(self, **kwargs: Any) -> None:
        super().__init__(**kwargs)

        if AsyncAzureOpenAI is None:
            raise ProviderError("openai is not installed. `pip install openai`.")

        api_key = kwargs.get("api_key") or os.environ.get("AZURE_OPENAI_API_KEY")
        endpoint = kwargs.get("endpoint") or os.environ.get("AZURE_OPENAI_ENDPOINT")
        deployment = kwargs.get("deployment") or os.environ.get("AZURE_OPENAI_DEPLOYMENT")
        api_version = kwargs.get("api_version") or os.environ.get("AZURE_OPENAI_API_VERSION") or "2024-11-20"

        if not api_key:
            raise ProviderError("AzureOpenAIProvider requires `api_key` (or AZURE_OPENAI_API_KEY).")
        if not endpoint:
            raise ProviderError("AzureOpenAIProvider requires `endpoint` (or AZURE_OPENAI_ENDPOINT).")
        if not deployment:
            raise ProviderError("AzureOpenAIProvider requires `deployment` (or AZURE_OPENAI_DEPLOYMENT).")

        self.model = deployment
        self.default_max_tokens = int(kwargs.get("max_completion_tokens") or 8000)
        base_url = endpoint.rstrip("/") + f"/openai/deployments/{deployment}"
        self._client = AsyncAzureOpenAI(api_key=api_key, api_version=api_version, azure_endpoint=endpoint)

    async def generate(self, messages: list[dict[str, Any]], tools: Iterable[dict[str, Any]] | None = None, context: dict[str, Any] | None = None) -> ModelResponse:
        kwargs = self._build_request_kwargs(messages, tools, context, stream=False)
        try:
            resp = await self._client.chat.completions.create(**kwargs)
        except Exception as exc:
            log.exception(f"Azure OpenAI generate failed for model {self.model}")
            raise ProviderError(f"Azure OpenAI API error: {exc!s}") from exc
        content = resp.choices[0].message.content if resp.choices else ""
        return ModelResponse(content=content or "", model=self.model, raw=resp, usage=self._usage_to_dict(getattr(resp, "usage", None)))

    async def stream(self, messages: list[dict[str, Any]], tools: Iterable[dict[str, Any]] | None = None, context: dict[str, Any] | None = None) -> AsyncIterator[StreamEvent]:
        kwargs = self._build_request_kwargs(messages, tools, context, stream=True)
        content_buf: list[str] = []
        try:
            stream = await self._client.chat.completions.create(**kwargs)
            async for chunk in stream:
                if not getattr(chunk, "choices", None):
                    continue
                delta = getattr(chunk.choices[0], "delta", None)
                text_delta = getattr(delta, "content", None) if delta is not None else None
                if text_delta:
                    content_buf.append(text_delta)
                    yield StreamEvent(delta=text_delta)
        except Exception as exc:
            log.exception("Azure OpenAI stream failed")
            yield StreamEvent(done=True, error=f"Azure OpenAI API error: {exc!s}")
            return
        yield StreamEvent(response=ModelResponse(content="".join(content_buf), model=self.model, raw=None, usage={}), done=True)

class OpenRouterProvider(AIProvider):
    """Custom OpenRouter provider — OpenAI-compatible, 100+ model endpoint.

    OpenRouter (https://openrouter.io) provides a unified API to access
    100+ models from different vendors (OpenAI, Anthropic, Meta, Mistral, etc.)
    using the OpenAI SDK. This provider configures the SDK to point at the
    OpenRouter base URL and handles request headers.

    Models are specified in the format: ``anthropic/claude-3-sonnet``,
    ``openai/gpt-4o``, ``meta-llama/llama-3-8b``, etc.
    See https://openrouter.io/docs/models for available models.
    """

    name = "openrouter"

    def __init__(self, **kwargs: Any) -> None:
        """Initialize the OpenRouter provider.

        Parameters
        ----------
        api_key : str, optional
            OpenRouter API key. If not provided, reads from ``OPENROUTER_API_KEY``
            environment variable. Required.
        model : str
            The model ID in OpenRouter format (e.g., ``anthropic/claude-3-5-sonnet``).
            Required. See https://openrouter.io/docs/models for available models.
        base_url : str, optional
            Override the OpenRouter API endpoint. Defaults to
            ``https://openrouter.io/api/v1``.
        max_completion_tokens : int, optional
            Default maximum tokens in the response. Defaults to 8000.
        """
        super().__init__(**kwargs)

        if AsyncOpenAI is None:
            raise ProviderError("openai is not installed. `pip install openai`.")

        # Get the API key
        api_key = kwargs.get("api_key") or os.environ.get("OPENROUTER_API_KEY")
        if not api_key:
            raise ProviderError(
                "OpenRouterProvider requires `api_key` (or OPENROUTER_API_KEY). "
                "Get one at https://openrouter.io/api_keys"
            )

        # Get the model name
        model = kwargs.get("model")
        if not model:
            raise ProviderError(
                "OpenRouterProvider requires `model` (e.g. 'anthropic/claude-3-5-sonnet'). "
                "See https://openrouter.io/docs/models for available models."
            )

        self.model = model
        self.default_max_tokens = int(kwargs.get("max_completion_tokens") or 8000)

        # Create the OpenAI client pointing at OpenRouter
        base_url = kwargs.get("base_url") or "https://openrouter.ai/api/v1"
        log.info(
            f"Initializing OpenRouter provider with model: {model}, base_url: {base_url}"
        )
        self._client = AsyncOpenAI(
            api_key=api_key,
            base_url=base_url,
        )

    async def generate(
        self,
        messages: list[dict[str, Any]],
        tools: Iterable[dict[str, Any]] | None = None,
        context: dict[str, Any] | None = None,
    ) -> ModelResponse:
        """Generate a completion (non-streaming).

        Parameters
        ----------
        messages : list[dict]
            Conversation messages in OpenAI format.
        tools : Iterable[dict], optional
            MCP tool definitions (not yet supported in this implementation).
        context : dict, optional
            Additional context (temperature, max_tokens, etc.).

        Returns
        -------
        ModelResponse
            The model's response with content and metadata.
        """
        kwargs = self._build_request_kwargs(messages, tools, context, stream=False)

        try:
            log.info(
                f"OpenRouter request: model={kwargs.get('model')}, max_tokens={kwargs.get('max_tokens')}"
            )
            resp = await self._client.chat.completions.create(**kwargs)
        except Exception as exc:  # pragma: no cover
            # Log the actual error for debugging
            log.exception(f"OpenRouter generate failed for model {self.model}")
            # Show user-friendly message for all AI failures
            raise ProviderError(f"OpenRouter API error: {exc!s}") from exc

        content = ""
        if resp.choices:
            content = resp.choices[0].message.content or ""

        return ModelResponse(
            content=content,
            model=self.model,
            raw=resp,
            usage=self._usage_to_dict(getattr(resp, "usage", None)),
        )

    async def stream(
        self,
        messages: list[dict[str, Any]],
        tools: Iterable[dict[str, Any]] | None = None,
        context: dict[str, Any] | None = None,
    ) -> AsyncIterator[StreamEvent]:
        """Stream a completion.

        Parameters
        ----------
        messages : list[dict]
            Conversation messages in OpenAI format.
        tools : Iterable[dict], optional
            MCP tool definitions (not yet supported in this implementation).
        context : dict, optional
            Additional context (temperature, max_tokens, etc.).

        Yields
        ------
        StreamEvent
            Text deltas and final response.
        """
        kwargs = self._build_request_kwargs(messages, tools, context, stream=True)
        content_buf: list[str] = []
        usage_payload: dict[str, Any] = {}

        try:
            stream = await self._client.chat.completions.create(**kwargs)
        except Exception:  # pragma: no cover
            # Log the actual error for debugging
            log.exception("OpenRouter stream failed")
            # Show user-friendly message for all AI failures
            yield StreamEvent(
                done=True,
                error="Something went wrong while generating your UI. Please try again in a moment.",
            )
            return

        try:
            async for chunk in stream:
                # Capture usage if present
                chunk_usage = getattr(chunk, "usage", None)
                if chunk_usage is not None:
                    usage_payload = self._usage_to_dict(chunk_usage)

                # Process choices
                if not getattr(chunk, "choices", None):
                    continue

                choice = chunk.choices[0]
                delta = getattr(choice, "delta", None)
                if delta is None:
                    continue

                # Emit text delta
                text_delta = getattr(delta, "content", None)
                if text_delta:
                    content_buf.append(text_delta)
                    yield StreamEvent(delta=text_delta)

        except Exception:  # pragma: no cover
            # Log the actual error for debugging
            log.exception("OpenRouter stream processing failed")
            # Show user-friendly message for all AI failures
            yield StreamEvent(
                done=True,
                error="Something went wrong while generating your UI. Please try again in a moment.",
            )
            return

        # Emit the final response
        yield StreamEvent(
            response=ModelResponse(
                content="".join(content_buf),
                model=self.model,
                raw=None,
                usage=usage_payload,
            ),
            done=True,
        )

    def _build_request_kwargs(
        self,
        messages: list[dict[str, Any]],
        tools: Iterable[dict[str, Any]] | None,
        context: dict[str, Any] | None,
        *,
        stream: bool,
    ) -> dict[str, Any]:
        """Build the kwargs for the OpenAI chat completions call."""
        for msg in messages:
            if msg.get("role") == "system":
                sys_content = msg.get("content", "")
                log.debug(
                    "System message in request: %d chars (head: %r)",
                    len(sys_content),
                    sys_content[:150],
                )
                break
        else:
            log.warning(
                "No system message in request - model will lack persona and rules."
            )

        kwargs: dict[str, Any] = {
            "model": self.model,
            "messages": messages,
            "temperature": 0.0,  # Deterministic for A2UI JSON generation
        }

        max_tokens = self.default_max_tokens
        if isinstance(context, dict) and "max_tokens" in context:
            try:
                max_tokens = int(context["max_tokens"])
            except (TypeError, ValueError):
                pass
        kwargs["max_tokens"] = max_tokens

        if isinstance(context, dict) and "temperature" in context:
            try:
                kwargs["temperature"] = float(context["temperature"])
            except (TypeError, ValueError):
                pass

        if stream:
            kwargs["stream"] = True

        return kwargs

    @staticmethod
    def _usage_to_dict(usage: Any) -> dict[str, Any]:
        """Convert usage object to dict."""
        if usage is None:
            return {}
        return {
            "prompt_tokens": getattr(usage, "prompt_tokens", 0),
            "completion_tokens": getattr(usage, "completion_tokens", 0),
            "total_tokens": getattr(usage, "total_tokens", 0),
        }
