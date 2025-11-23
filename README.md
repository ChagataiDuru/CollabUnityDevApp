# CollabUnityDevApp

A comprehensive Unity Game Development Collaboration Hub that streamlines team collaboration, project management, and version control for Unity game development teams.

## 🎮 Features

- **Project Management**: Kanban boards, sprint planning, and task tracking
- **Team Collaboration**: Real-time whiteboard, team chat, and member management
- **Version Control Integration**: Git integration with commit tracking and branch management
- **Time Tracking**: Track time spent on tasks with manual and automatic logging
- **Sprint Management**: Plan and track sprints with velocity metrics
- **Task Management**: Create, assign, and track tasks with priorities and statuses

## 🏗️ Architecture

This application is built with a modern full-stack architecture:

### Frontend
- **Framework**: Angular 18+
- **UI Components**: Angular Material
- **Styling**: Tailwind CSS
- **State Management**: RxJS
- **Real-time Features**: SignalR

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT-based authentication
- **Real-time Communication**: SignalR

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Reverse Proxy**: Nginx
- **Database**: PostgreSQL 15

## 🚀 Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- [Node.js](https://nodejs.org/) (v18 or higher) - for local development
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) - for local development

### Running with Docker (Recommended)

1. Clone the repository:
```bash
git clone git@github.com:ChagataiDuru/CollabUnityDevApp.git
cd CollabUnityDevApp
```

2. Create a `.env` file in the root directory (use `.env.example` as template if available)

3. Start the application:
```bash
docker-compose up --build
```

4. Access the application:
   - Frontend: http://localhost:80
   - Backend API: http://localhost:5000
   - Database: localhost:5432

### Local Development

#### Backend Setup

```bash
cd backend/UnityDevHub.API
dotnet restore
dotnet run
```

#### Frontend Setup

```bash
cd frontend
npm install
npm start
```

## 📁 Project Structure

```
CollabUnityDevApp/
├── backend/
│   └── UnityDevHub.API/
│       ├── Controllers/      # API endpoints
│       ├── Services/         # Business logic
│       ├── Models/           # Data models
│       ├── Data/             # Database context
│       └── Hubs/             # SignalR hubs
├── frontend/
│   └── src/
│       ├── app/
│       │   ├── core/         # Core services and models
│       │   ├── features/     # Feature modules
│       │   └── shared/       # Shared components
│       └── assets/           # Static assets
├── nginx/                    # Nginx configuration
└── docker-compose.yml        # Docker orchestration
```

## 🔧 Configuration

### Environment Variables

Create a `.env` file in the root directory with the following variables:

```env
# Database
POSTGRES_USER=your_db_user
POSTGRES_PASSWORD=your_db_password
POSTGRES_DB=unitydevhub

# Backend
ASPNETCORE_ENVIRONMENT=Development
JWT_SECRET=your_jwt_secret_key
```

## 🧪 Testing

### Backend Tests
```bash
cd backend/UnityDevHub.API
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## 📝 API Documentation

Once the application is running, you can access the Swagger API documentation at:
```
http://localhost:5000/swagger
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- **Chaghatai Duru** - [ChagataiDuru](https://github.com/ChagataiDuru)

## 🙏 Acknowledgments

- Built with Angular and ASP.NET Core
- UI components from Angular Material
- Styling with Tailwind CSS
