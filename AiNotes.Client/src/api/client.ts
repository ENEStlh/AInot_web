const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5100/api'

let accessToken: string | null = null

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export const tokenStore = {
  set(token: string) {
    accessToken = token
  },
  clear() {
    accessToken = null
  },
  get() {
    return accessToken
  },
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Accept', 'application/json')

  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  } catch {
    throw new ApiError(0, 'API sunucusuna bağlanılamadı. Önce backend için dotnet run komutunu çalıştırın.')
  }
  if (response.status === 204) {
    return undefined as T
  }

  const raw = await response.text()
  let payload: unknown
  try {
    payload = raw ? JSON.parse(raw) : undefined
  } catch {
    payload = undefined
  }

  if (!response.ok) {
    const problem = payload as { detail?: string; title?: string } | undefined
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? 'İstek başarısız oldu.')
  }

  return payload as T
}

export type AuthResponse = { token: string; expiresAt: string }
export type Note = {
  id: string
  title: string
  content: string
  summary: string | null
  tags: string[]
  createdAt: string
  updatedAt: string
}
export type NoteListResponse = {
  items: Note[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
export type TagSuggestionResponse = { tags: string[] }

export const api = {
  register(email: string, password: string) {
    return request<void>('/Auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
  },
  login(email: string, password: string) {
    return request<AuthResponse>('/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
  },
  listNotes(search: string, page: number, pageSize = 10) {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    if (search.trim()) params.set('search', search.trim())
    return request<NoteListResponse>(`/Notes?${params.toString()}`)
  },
  getNote(id: string) {
    return request<Note>(`/Notes/${id}`)
  },
  createNote(title: string, content: string) {
    return request<Note>('/Notes', {
      method: 'POST',
      body: JSON.stringify({ title, content }),
    })
  },
  updateNote(id: string, title: string, content: string) {
    return request<Note>(`/Notes/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ title, content }),
    })
  },
  deleteNote(id: string) {
    return request<void>(`/Notes/${id}`, { method: 'DELETE' })
  },
  summarize(id: string) {
    return request<Note>(`/Notes/${id}/summarize`, { method: 'POST' })
  },
  suggestTags(id: string) {
    return request<TagSuggestionResponse>(`/Notes/${id}/suggest-tags`, { method: 'POST' })
  },
}
