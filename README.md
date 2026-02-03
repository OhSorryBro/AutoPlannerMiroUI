Auto Planner MIRO

Author: Michał Domagała
Version: 1.3
Status: Working Proof-of-Concept

<img width="688" height="303" alt="Auto Planner Miro" src="https://github.com/user-attachments/assets/ab841fc9-0973-4c90-90bd-a50abd8b9170" />

Project Description

Auto Planner MIRO is a desktop application (WinForms) created as a proof-of-concept for automatic planning and visualization of warehouse order preparation.
The tool simulates allocation of orders to formeren stations (workstations) and ready locations (docks), with live output to a MIRO board through the MIRO API.
Order categories, colors, and time parameters are hardcoded in the application, while user-supplied CSV files are used for additional constraints.



Key Features

  Dynamic planning with constraints

    Order priorities (priority list from CSV)

    Time windows (loading/start/end in minutes)

    Operator and workstation availability

    Blocking docks at specific times (from CSV)

    Validation rules (minimum two docks per operator)

  Priority & severity handling

    Priority orders can override standard allocation rules

    Severity levels influence visualization and allocation

    Error handling when constraints cannot be respected

  CSV-based configuration (limited scope)

    priority_orders.csv → defines priority orders

    docks.csv → defines blocked docks and times

  MIRO API integration

    Creates shapes on a MIRO board for each order

    Colors/shapes depend on category and severity

    Uses RestSharp for communication with MIRO

  User Interface (WinForms)

    Wizard for choosing:

      number of stations

      number of ready locations

      layout type (H/K)

    Console-like logging panel (“Terminal”)

  Technologies

    C# / .NET (WinForms)

    RestSharp (HTTP calls to MIRO API)

    Microsoft.Extensions.Configuration (appsettings.json support)

    CSV input (priority orders, dock blocking)

    MIRO API

  Installation & Setup

    Download or clone the repository.

    Prepare the CSV files if needed:

      priority_orders.csv – leave only header row if you want to disable priorities.

      docks.csv – optional, defines blocked docks and times.

  Update appsettings.json with:

    MIRO API key

    MIRO board ID

    Application parameters (colors, layout settings – if used).

  Make sure you have the required version of .NET Framework installed.

    Run the application.

  Usage Notes

    ⚠️ Each operator must have at least 2 docks available.
    Do not block docks in a way that leaves fewer than 2 open – this will cause assignment errors.

    ⚠️ Too many priority orders may break planning.
    The algorithm assumes priority orders are rare and exceptional.

    ⚠️ To disable priority orders:
    Clear all rows (except the header) in priority_orders.csv.
