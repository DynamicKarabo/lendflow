import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import { AuthProvider, useAuth } from "@/hooks/useAuth"
import { AppShell } from "@/components/layout/AppShell"
import Dashboard from "@/pages/Dashboard"
import Loans from "@/pages/Loans"
import LoanDetail from "@/pages/LoanDetail"
import Applications from "@/pages/Applications"
import Applicants from "@/pages/Applicants"
import Login from "@/pages/Login"

function RequireAuth({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/*"
        element={
          <RequireAuth>
            <AppShell>
              <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/loans" element={<Loans />} />
                <Route path="/loans/:id" element={<LoanDetail />} />
                <Route path="/applications" element={<Applications />} />
                <Route path="/applicants" element={<Applicants />} />
              </Routes>
            </AppShell>
          </RequireAuth>
        }
      />
    </Routes>
  )
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
