import { createRoot } from 'react-dom/client'
import './index.css'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { ToastContainer } from 'react-toastify'
import HomePage from './components/HomePage.jsx'
import RegisterPage from './components/RegisterPage.jsx'
import LoginPage from './components/LoginPage.jsx'
import UsersManagementPage from './components/UsersManagmentsPage.jsx'
import UserVersionsPage from './components/UserVersionsPage.jsx'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={ <HomePage /> } />
        <Route path="/register" element={ <RegisterPage /> } />
        <Route path="/login" element={ <LoginPage /> } />
        <Route path="/admin" element={ <UsersManagementPage /> } />
        <Route path="/admin/userVersions/:id" element={ <UserVersionsPage /> } />
      </Routes>
      <ToastContainer />
    </BrowserRouter>
  )
}

createRoot(document.getElementById("root")).render(<App />);
