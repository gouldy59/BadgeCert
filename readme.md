# Badge Management System

## Overview

A React-based web application for managing OpenBadges v3.0 digital credentials. The system allows users to import, validate, display, and share digital badges that comply with the OpenBadges v3.0 specification. Users can download badges as PNG/PDF files and share them directly to LinkedIn.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Frontend Architecture
- **React 19** with TypeScript for the user interface
- **Vite** as the build tool and development server
- **Bootstrap 5** for responsive UI components and styling
- **Font Awesome** for iconography
- **Axios** for HTTP client requests
- **Lucide React** for modern icon components

The frontend follows a component-based architecture with:
- Main App component handling authentication state
- Separate components for Login, Dashboard, Badge management
- Service layer for API communication and business logic
- Utility modules for file downloads and external integrations

### Component Structure
- `App.tsx` - Root component with authentication flow
- `Login.tsx` - User authentication interface
- `Dashboard.tsx` - Main application interface with badge/results tabs
- `BadgeCard.tsx` - Individual badge display and actions
- `AddBadgeModal.tsx` - Form for importing new badges with JSON validation

### Data Management
- Client-side state management using React hooks
- Local storage for authentication tokens
- Real-time validation of OpenBadges v3.0 JSON format
- Type-safe interfaces for Badge and Result entities

### Authentication System
- Token-based authentication with localStorage persistence
- Automatic authentication status checking on app load
- Protected routes requiring valid authentication

### Badge Processing
- JSON validation for OpenBadges v3.0 compliance
- Required context validation for credential standards
- Type checking for VerifiableCredential and OpenBadgeCredential
- Support for badge metadata including issuer, subject, and validity periods

## External Dependencies

### Backend API
- REST API expected at `http://localhost:8000/api`
- Endpoints for authentication, badge CRUD operations, and file generation
- Bearer token authentication for protected endpoints

### Third-Party Services
- **LinkedIn Sharing API** - For social media badge sharing
- **Bootstrap CDN** - UI component framework
- **Font Awesome CDN** - Icon library

### File Generation Services
- Backend services for PNG badge image generation
- PDF certificate generation for formal credentials
- Blob download utilities for client-side file handling

### Development Tools
- **Vite** development server with hot module replacement
- **TypeScript** for type safety and developer experience
- **ESNext** module system with bundler resolution

The application is designed to work with a separate backend service that handles data persistence, file generation, and authentication. The frontend focuses on user experience and OpenBadges standard compliance validation.