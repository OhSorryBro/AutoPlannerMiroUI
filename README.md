**Auto Planner MIRO**

Author: Michał Domagała

Version: 1.2

Status: Working Proof-of-Concept

<img width="688" height="303" alt="Auto Planner Miro" src="https://github.com/user-attachments/assets/ab841fc9-0973-4c90-90bd-a50abd8b9170" />

**Project Description**
MIRO Automatic Planner is an desktop application designed to automate and optimize the allocation of orders to operators and workstations in a logistics/warehouse environment. 
The project was delivered as a proof-of-concept, simulating real operations in a 3PL warehouse.

**Key Features**

  Dynamic planning algorithms considering:
    
    *Order priorities
    
    *Time constraints (loading time)
    
    *Maximum number of simultaneous loadings
    
    *Availability of operators and workstations
    
    *Integration with the MIRO API for live visualization of the planning process on MIRO boards
    
    *Automatic import of configuration data (e.g., number of workstations, order types, resource availability, blocking docks at certain times) from CSV file or terminal
    
    *Reporting and CSV export module for analysis and documentation, with possible integration with Power BI or other analytical tools
  
  Applied Heuristics
  
    *Priority rules for task allocation
    
    *Algorithms minimizing operator waiting time
    
    *Algorithms optimizing workstation utilization
    
    *Severity levels
    
  Technologies
    
    *C#, .NET
    
    *WinForms
    
    *RestSharp
    
    *Microsoft.Extensions.Configuration
    
    *MIRO API
  
  Before running the application, make sure that:
    
    *You have generated an appropriate CSV file with configuration data
    
    *The required version of .NET Framework is installed
  
  Installation
    
    *Download the repository from GitHub
    
    *Prepare the CSV configuration file according to the provided template
    
    *Configure the appsettings.json file with the necessary settings (drop me a message if you wish to get it)
    
    *Generate proper MIRO board (drop me a message if you wish to get it)
    
    *Run the application

**Note**

  !!!IMPORTANT: Each operator must have at least 2 available docks assigned. Do not block docks if it would leave the operator with less than two available, to avoid assignment errors.!!!
