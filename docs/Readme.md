# TsmXiL Plugin

This is a plugin to model Vehicle-in-Loop in TransModeler traffic simulation. It is a class library (.dll) project.

## Load the plugin DLL in TransModeler

The plugin dll is loaded in TransModeler using the TsmXiL.dbd resource. To load in TransModeler,

- Go to Simulation > Options... menu item and in the Call Every Simulation section in the Macros tab specify -
    1. Choose the TsmXiL.dbd file as UI Database by clicking on the "Choose" button
    2. *InitializeTsmXiLPlugin* macro for "At Start"
    3. *DisconnectTsmXiLPlugin* macro for "At End"

- Click on the "OK" button to save the changes

- Click the "Save" menu item from the main TransModeler menu to save the simulation project

   ![image info](images/simOptions.PNG)

## Plugin functionality

The Manager.cs class connects this plugin with the TransModeler API and handles simulation events. The Start() method in Manager.cs is the entry point into the plugin code.

The Controller.cs class has an Update() method which is called during each simulation timestep which can be set to a custom value as desired. The Update() method internally calls UpdateRealVehicle() method which sends speed and acceleration commands to the real vehicle controller and receives acceleration and speed in response. It then calls the UpdateSimulationVehicle() method which updates the speed and acceleration of the simulated vehicle.

The Controller.cs class also has an AddVehicle() method which is called to add a new vehicle to the simulation. This method creates a new instance of the Vehicle class and adds it to the list of vehicles with the attributes specified in the configuration file (config.txt). 

## Configuration

### Vehicle attributes

The config.txt file currently allows specifying the following attributes for the vehicle to be added -

1. Origin (VehOriginLaneId)
2. Destination (VehDestinationLaneId)
3. Initial speed (VehInitialSpeed)
4. Default acceleration (DefaultAcceleration)

### Controller attributes

The configuration file also provides the following configuration options -

1. Controller TCP IP Address (TcpAddress)
2. Controller TCP Port (TcpPort)

### Simulation attributes

1. Simulation timestep (UpdateInterval)
