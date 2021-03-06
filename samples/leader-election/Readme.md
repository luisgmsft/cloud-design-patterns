﻿# Leader Election Pattern

This document describes the Leader Election Pattern example from the guide [Cloud Design Patterns](http://aka.ms/Cloud-Design-Patterns).

## System Requirements

* Microsoft .NET Framework version 4.6
* Microsoft Visual Studio 2015 Comunity, Enterprise, or Professional with Update 2 or later
* Windows Azure SDK for .NET version 2.9
* ***[addition]*** [Install the Service Fabric runtime, SDK, and tools for Visual Studio 2015](http://www.microsoft.com/web/handlers/webpi.ashx?command=getinstallerredirect&appid=MicrosoftAzure-ServiceFabric-VS2015)

## Before you start

Ensure that you have installed all of the software prerequisites.

***[review]*** The example demonstrates operational aspects of applications running in Windows Azure. Therefore, you will need to use the diagnostics tools in order to understand how the code sample works. You **must** ensure that the web and worker roles in the solution are configured to use the diagnostics mechanism. If not, you will not see the trace information generated by the example.

## About the Example

***[review]*** This example shows how a worker role instance can become a leader among a group of peer instances. The leader can then perform tasks that coordinate and control the other instances; these tasks should be performed by only one instance of the worker role. The leader is elected by acquiring a blob lease..


## Running the Example

You can run this example locally in the Visual Studio Windows Azure emulator. You can also run this example by deploying it to a Windows Azure Cloud Service.

* Start Visual Studio using an account that has Administrator privileges ("Run as Administrator").
* Open the solution you want to explore from the subfolders where you downloaded the examples.
* ***[not-required]*** Right-click on each role in Solution Explorer, select Properties, and ensure that the role is configured to generate diagnostic information.

* ***[review-as-service-fabric-local-cluster]*** If you want to run the example in the local Windows Azure emulator:
	* ***[addition]*** Start the Microsoft Azure Storage Emulator on your machine.
    * Press F5 in Visual Studio to start the example running.
	* Open the Windows Azure Compute Emulator UI from the icon in the notification area.
	* Select each role in turn and view the diagnostic information generated by Trace statements in the code.
* ***[review-as-service-fabric-publishing]*** If you want to run the example on Windows Azure:
	* Provision a Windows Azure Cloud Service and deploy the application to it from Visual Studio.
	* Open the Server Explorer Window in Visual Studio and expand the Windows Azure entry.
	* Expand Cloud Services and then expand the entry for the solution you deployed.
	* Right-click each role instance and select View Diagnostics Data to see the diagnostic information generated by Trace statements in the code. This is written to the WADLogsTable in the Storage/Development/Tables section.
