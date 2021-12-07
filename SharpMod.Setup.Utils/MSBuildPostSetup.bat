call "%VS90COMNTOOLS%\vsvars32.bat"
msbuild /target:RunModifySilverLight PostSetupTasks.proj
msbuild /target:RunModifyWindows PostSetupTasks.proj
msbuild /target:RunCreateSilverLightSolution PostSetupTasks.proj
msbuild /target:RunCreateWindowsSolution PostSetupTasks.proj
msbuild /target:RunBatchCopySLUI PostSetupTasks.proj
msbuild /target:RunBatchCopySLDemo PostSetupTasks.proj

