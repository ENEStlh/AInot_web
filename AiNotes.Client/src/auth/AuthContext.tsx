import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import { api, tokenStore } from '../api/client'

type AuthContextValue = {
  token: string | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null)

  const value = useMemo<AuthContextValue>(() => ({
    token,
    async login(email, password) {
      const response = await api.login(email, password)
      tokenStore.set(response.token)
      setToken(response.token)
    },
    async register(email, password) {
      await api.register(email, password)
    },
    logout() {
      tokenStore.clear()
      setToken(null)
    },
  }), [token])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
