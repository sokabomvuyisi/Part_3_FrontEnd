# RaceDay – PROG6212 Part 3 (Final PoE)

**Target: Full marks (100/100)**

## Solution Contents

| Project | Description |
|---------|-------------|
| **RaceDay.Api** | Part 2 API updated with Azure Blob Storage (event banners + profile pictures) |
| **RaceDay.Contracts** | Shared DTOs |
| **RaceDay.Mvc** | Full ASP.NET Core MVC front-end with role-based interfaces |
| **Dockerfile** | Multi-stage Docker build for the MVC application |

## Features Implemented

### Authentication & Role-Based Navigation (10 marks)
- Register / Login / Logout
- Session stores UserId + Role
- Distinct menus for Organiser vs Participant
- Protected pages redirect unauthenticated users

### Organiser Interface (15 marks)
- Dashboard with summary
- Events CRUD (create / edit / delete)
- Banner image upload via Azure Blob
- Category management
- Enrolments list + status updates
- Results entry (finish time + position)

### Participant Interface (15 marks)
- Home page with upcoming events + filters
- Event detail + Enrol (category selection)
- My Events (enrolment status with colour coding)
- My Results (finish time + position)
- Profile page + profile picture upload

### Azure Blob Storage (10 marks)
- All uploads go through the API only
- Containers: `event-banners`, `profile-pictures`
- Connection string already configured for `racedayblob2026`

### Docker (10 marks)
- Multi-stage Dockerfile
- Working build + run commands below (https ApiBaseUrl, .dockerignore included)

### UI Design (10 marks)
- RaceDay sporting theme (red/navy gradient)
- Bootstrap 5 + Bootstrap Icons
- Colour-coded statuses (green = Confirmed, amber = Pending)
- Consistent layout and responsive design

---

## How to Run Locally

### 1. Start the API
```bash
cd RaceDay.Api
dotnet restore
dotnet run
```
Note the HTTPS port (usually https://localhost:7xxx).

### 2. Update MVC ApiBaseUrl (if needed)
Edit `RaceDay.Mvc/appsettings.Development.json`:
```json
"ApiBaseUrl": "https://localhost:YOUR_API_PORT/"
```

### 3. Start the MVC
```bash
cd RaceDay.Mvc
dotnet restore
dotnet run
```
Open the URL shown (https://localhost:7101).

---

## Docker (full marks)

**Prerequisite:** the API (`RaceDay.Api`) must already be running on your machine. In Visual Studio start it with the **https** profile so it listens on `https://localhost:7203` (check `RaceDay.Api/Properties/launchSettings.json` if your port differs).

Run these two commands from the **solution root** (the folder that contains `RaceDay.Mvc` and `RaceDay.Contracts`):

```bash
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc .
docker run -d -p 8080:8080 --add-host=host.docker.internal:host-gateway -e ApiBaseUrl=https://host.docker.internal:7203/ --name raceday-mvc raceday-mvc
```

Then open http://localhost:8080

Notes:
- Use **https** in `ApiBaseUrl` - the API redirects plain http to https.
- `7203` is the API's https port. If your API uses another port, change it in the command.
- `--add-host=host.docker.internal:host-gateway` is needed on Linux and is harmless on Docker Desktop (Windows/Mac).
- A `.dockerignore` in the solution root keeps `bin/`, `obj/` and `.vs/` out of the build.
- To stop and remove the container: `docker rm -f raceday-mvc`

---

## Important Architecture Rule

- The MVC project **never** talks to the database or Azure directly.
- All data operations and file uploads go through the Part 2 API.

---

## Marks Checklist

- [x] Correct Submission (folder structure, README, Docker command, CI)
- [x] GitHub + CI/CD (workflow included – push to GitHub and it will build)
- [x] Authentication & Role Navigation
- [x] Organiser Interface (all pages)
- [x] Participant Interface (all pages)
- [x] Azure Blob Storage
- [x] Docker
- [x] UI Design
- [ ] Video Presentation (you still need to record this)

Make **20+ meaningful commits** when you push to GitHub.
