# SkyBook Flight Booking Sample (.NET MAUI)

A production-ready .NET MAUI application for AI-powered flight booking using Syncfusion A2UI (AI-to-UI), Syncfusion MAUI controls, and Azure OpenAI.

## Overview

**SkyBook** is an enterprise flight booking assistant that uses AI to guide users through a 3-stage booking journey:

1. **Stage 1 - Search**: User specifies flight preferences such as route, travel dates, cabin class, and passengers.
2. **Stage 2 - Results**: AI displays matching flights and allows users to select a preferred option.
3. **Stage 3 - Confirmation**: User reviews booking details and completes the booking workflow.

The application combines a conversational AI assistant with dynamically rendered booking screens powered by Syncfusion A2UI. Users can interact through chat while the application automatically updates the booking interface based on AI responses.

---

## Prerequisites

### System Requirements

- Visual Studio 2022 (17.14 or later recommended)
- .NET 10 SDK (or the version used by the project)
- .NET MAUI workload installed
- Windows 10 version 19041 or later
- Syncfusion License Key

### AI Requirements

To use AI-powered responses, configure Azure OpenAI:

- Azure OpenAI Endpoint
- Azure OpenAI API Key
- Azure OpenAI Deployment Name

These values can be configured in `MauiProgram.cs`.

---

## Installation & Setup

### 1. Navigate to the MAUI Project

```bash
cd flight-booking/client/maui
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Azure OpenAI

Open `MauiProgram.cs` and update:

```csharp
var ai = new SkyBook.Core.AzureOpenAiOptions(
    Endpoint: "YOUR_ENDPOINT",
    ApiKey: "YOUR_API_KEY",
    Deployment: "YOUR_DEPLOYMENT_NAME");
```

### 4. Configure Syncfusion License

Register your Syncfusion license key in the application configuration.

---

## Running the Application

### Using Visual Studio

1. Open the solution.
2. Set **SkyBook.Maui** as Startup Project.
3. Select:
   - Windows Machine
   - Android Emulator
   - iOS Simulator (Mac required)
4. Press **F5**.
---

## Application Workflow

### Flight Search

The AI assistant helps users enter:

- Origin
- Destination
- Departure Date
- Return Date
- Cabin Class
- Passenger Details

Once completed, the AI renders a dynamic search form.

### Flight Results

After searching:

- Available flights are displayed.
- Users can compare options.
- A preferred flight can be selected for booking.

### Booking Confirmation

The final stage allows users to:

- Review booking information.
- Verify traveler details.
- Confirm the booking.

---

## Features

### AI-Powered Assistant

The built-in **SkyBook Assistant** provides:

- Natural language flight search
- Context-aware booking assistance
- Guided multi-step workflow
- Dynamic UI generation

The application uses **Syncfusion SfChat** for conversational interactions and maintains chat history during the booking session.

### Dynamic A2UI Rendering

The booking interface is generated dynamically using:

- SurfaceHost
- SurfaceModel
- Syncfusion A2UI Components

This allows the AI to render booking screens without hardcoding every page.

### Responsive Experience

The application automatically adapts for:

- Desktop
- Tablet
- Mobile

Different layouts are applied based on screen width.

### Booking Progress Tracker

Users can visually track progress through:

- Search
- Results
- Confirmation

using a built-in booking stepper.

---

## Project Structure

```text
flight-booking/client/maui/

├── App.xaml
├── App.xaml.cs
├── MauiProgram.cs
├── MainPage.xaml
├── MainPage.xaml.cs
├── ChatViewModel.cs
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
├── designs/
│   ├── stage1-flight-search.json
│   ├── stage2-flight-results.json
│   └── stage3-booking-confirmation.json
└── Platforms/
    ├── Android/
    ├── iOS/
    ├── MacCatalyst/
    └── Windows/
```

Based on the current implementation, the application uses `MainPage.xaml.cs` as the primary booking workflow page and `ChatViewModel.cs` for chat state management.

---

## Technologies Used

- .NET MAUI
- C#
- XAML
- Syncfusion MAUI Controls
- Syncfusion SfChat
- Syncfusion A2UI
- Azure OpenAI
- Dependency Injection

---

## Troubleshooting

### Application cannot connect to AI service

**Cause**

Azure OpenAI configuration is missing or invalid.

**Solution**

Verify:

```csharp
Endpoint
ApiKey
Deployment
```

values in `MauiProgram.cs`

---

### Package restore failures

**Solution**

Clear NuGet cache and restore:

```bash
dotnet nuget locals all --clear
dotnet restore
```

---

### Windows build fails with long path errors

**Cause**

Project location path is too long.

**Solution**

Move the repository to a shorter location such as:

```text
D:\Projects\FlightBooking
```

Then delete:

```text
bin
obj
```

and rebuild.

---

### A2UI screen does not render

**Cause**

Invalid or incomplete A2UI surface definition.

**Solution**

Verify:

- JSON design files
- Surface creation messages
- Component definitions

The application logs rendering failures and falls back gracefully where possible. 【1-56ac79】

---

## Learn More

- Syncfusion MAUI Controls
- Syncfusion A2UI
- Azure OpenAI
- .NET MAUI Documentation

---

## Summary

SkyBook demonstrates how to build an AI-powered flight booking experience using .NET MAUI, Syncfusion A2UI, Syncfusion MAUI controls, and Azure OpenAI. The sample combines conversational AI with dynamically generated booking interfaces to create a modern travel booking workflow. 【2-715df2】【1-56ac79】【3-22a2ea】