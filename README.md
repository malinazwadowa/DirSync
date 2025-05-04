# DirSync

## 📋 About

**DirSync** is a simple C# console app that syncs two folders: **source** and **replica**.  
It makes sure the **replica** folder is always a mirror of the **source** folder with one-way sync. The app runs on a set interval, handling file copying, deletions, and errors, all while keeping a log of what's going and optional archiving just in case. 


---

## ⚙️ Features

- One-way synchronization from source to replica
- Periodic sync at custom intervals
- Logs operations to both console and files
- Supports interactive and non-interactive modes
- Optional file archiving before deletion
- Robust config validation and CLI argument parsing

---

## 🚀 How to Run

- Download latest release .exe from releases page 

### ⚙️ Available Parameters

| Parameter       | Description                                                | Example                                      |
|-----------------|------------------------------------------------------------|----------------------------------------------|
| `--source`      | Path to the source directory                               | `--source=C:\source`                         |
| `--replica`     | Path to the replica (destination) directory                | `--replica=D:\replica`                       |
| `--logs`        | Path where directory with logs and archive will be created | `--logs=C:\Desktop`                          |
| `--interval`    | Sync interval in seconds                                   | `--interval=3600`                            |
| `--archive`     | Enable archiving of deleted files                          | `--archive=true`                             |
| `--i`           | Start in interactive (walkthrough) mode                    | `--i`                                        |
| `--help`        | Show help with usage info                                  | `--help`                                     |



### 🏁 Usage

<br>

During the first launch, the config.json will be created in the executable directory. On subsequent launches, the app will attempt to use this existing configuration file.
Config example:
```bash
{
  "SourcePath": "D:\\Test\\Origin",
  "ReplicaPath": "D:\\Test\\Target",
  "LogFilePath": "D:\\Test\\",
  "SyncIntervalSeconds": 1800,
  "ArchiveEnabled": false
}
```
<br>


▶️ **One-liner (non-interactive):** 
```bash
.\DirSync.exe --source=C:\Source --replica=D:\Replica --logs=C:\Logs --interval=3600 --archive=true
```
If valid config file is available arguments can be skipped. If specified, new data will be entered into config file.
```bash
.\DirSync.exe --replica=D:\NewReplica
```
<br>

🧑‍💻 **Interactive mode (guided setup):**
```bash
.\DirSync.exe --i
```

<br>

### 📚 Additional Information
- Each synchronization session generates its own dedicated log and archive folder. Only if there is something to log or archive.
- During synchronization, the app will recreate the folder structure for any files being backed up.
- If the app is unable to complete the sync before the specified interval ends, it will notify the user of the delay upon sync completion and immediately start the next synchronization session.

<br>

## 🏗 Architecture

- Program  
  Orchestrates core components and kicks off the synchronization loop.

- ConfigManager  
  Loads, validates, merges, and persists application settings via `config.json` and CLI.

- SyncScheduler  
  Runs the sync service on a regular timer, including startup countdown and delay handling.

- SyncService  
  Executes the core sync steps—detects differences, copies new/updated files.

- Config  
  Data model for app settings (paths, interval, archive flag) with defaults and JSON (de)serialization.
  
- Logger  
  Writes info and error messages to console and log files, with optional per-session logs.

- PromptUtils  
  Interactive helper for collecting and validating user input (paths, intervals, confirmations).

- FileSystemUtils  
  Performs safe file and directory operations, includes logging.
  
- OperationStatus  
  Simple flag tracker for operation success or failure.
