import { createContext, useContext, useState, useCallback, type ReactNode } from "react"
import type { LoginResponse } from "@/types"

interface AuthState {
  token: string | null
  email: string | null
  role: string | null
  isAuthenticated: boolean
}

interface AuthContextType extends AuthState {
  login: (res: LoginResponse) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextType>({
  token: null,
  email: null,
  role: null,
  isAuthenticated: false,
  login: () => {},
  logout: () => {},
})

function getStoredAuth(): AuthState {
  const token = localStorage.getItem("lendflow_token")
  const email = localStorage.getItem("lendflow_email")
  const role = localStorage.getItem("lendflow_role")
  return {
    token,
    email,
    role,
    isAuthenticated: !!token,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(getStoredAuth)

  const login = useCallback((res: LoginResponse) => {
    localStorage.setItem("lendflow_token", res.Token)
    localStorage.setItem("lendflow_email", res.Email)
    localStorage.setItem("lendflow_role", res.Role)
    setState({ token: res.Token, email: res.Email, role: res.Role, isAuthenticated: true })
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem("lendflow_token")
    localStorage.removeItem("lendflow_email")
    localStorage.removeItem("lendflow_role")
    setState({ token: null, email: null, role: null, isAuthenticated: false })
  }, [])

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
