Macro "InitializeTsmXiLPlugin" (args)
    shared manager
    folder = "c:\\temp\\tsmxil\\"
    dllFile = folder + "TsmXiL.dll"
    configFile = folder + "config.txt"
    manager = CreateManagedObject(dllFile, "TsmXiL.Manager",)
    if manager != null then do
        
        started = manager.Start(configFile)
   
        if !started then do
            eMsg = "TsmXiL plugin could not be started! "
            if manager.LogFile != null then
                eMsg = eMsg + "\n\nPlease see " + manager.LogFile + " for more information."
            ShowMessage(eMsg)
            SetSimulationStatus(-4) //quit because of error
        end
    end
endMacro

Macro "DisconnectTsmXiLPlugin"
    shared manager
    if manager != null then do
        manager.Disconnect()
        manager = null
    end
    gc = GetManagedClass(, "System.GC",)
    gc.Collect()
endMacro