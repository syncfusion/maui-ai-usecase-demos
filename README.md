# maui-ai-usecase-demos

This repository contains a collection of AI-focused .NET MAUI samples. Some samples demonstrate Syncfusion AI AssistView scenarios, while others showcase AI-adjacent app flows and control integrations used in practical app experiences.

The HomeInventoryManager sample uses a deterministic mock AI service by default, so it runs without any Azure OpenAI setup. The DI container is already wired so the mock can be replaced with a real `IAIService` implementation later without changing the pages.
