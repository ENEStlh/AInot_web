import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { Link, Navigate, NavLink, Outlet, Route, Routes, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, ArrowRight, Check, ChevronLeft, ChevronRight, FileText, Hash, LayoutDashboard, LoaderCircle, LogOut, Menu, Plus, Search, Sparkles, Tag, Trash2, X } from 'lucide-react'
import { ApiError, api, type Note } from './api/client'
import { useAuth } from './auth/AuthContext'

function Spinner({ small = false }: { small?: boolean }) {
  return <LoaderCircle className={`spin ${small ? 'h-4 w-4' : 'h-5 w-5'}`} />
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<AuthPage mode="login" />} />
      <Route path="/register" element={<AuthPage mode="register" />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<Shell />}>
          <Route path="/notes" element={<NotesPage />} />
          <Route path="/notes/new" element={<EditorPage />} />
          <Route path="/notes/:id" element={<EditorPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/notes" replace />} />
    </Routes>
  )
}

function ProtectedRoute() {
  const { token } = useAuth()
  return token ? <Outlet /> : <Navigate to="/login" replace />
}

function Shell() {
  const { logout } = useAuth()
  const [mobileMenu, setMobileMenu] = useState(false)

  return (
    <div className="paper-grid min-h-screen lg:flex">
      <aside className={`${mobileMenu ? 'flex' : 'hidden'} fixed inset-0 z-20 flex-col bg-[#173c36] p-6 text-[#f7f1e5] lg:relative lg:flex lg:w-[280px] lg:shrink-0`}>
        <div className="flex items-start justify-between">
          <Link to="/notes" className="flex items-center gap-3" onClick={() => setMobileMenu(false)}>
            <span className="grid h-10 w-10 place-items-center bg-[#e9bb4b] text-[#173c36]"><Sparkles size={20} /></span>
            <span><strong className="block text-lg tracking-tight">AI Notes</strong><small className="font-mono text-[10px] uppercase tracking-[.24em] text-[#aac7b6]">düşünce alanı</small></span>
          </Link>
          <button className="lg:hidden" onClick={() => setMobileMenu(false)} aria-label="Menüyü kapat"><X /></button>
        </div>
        <nav className="mt-16 space-y-2">
          <NavLink to="/notes" onClick={() => setMobileMenu(false)} className={({ isActive }) => `flex items-center gap-3 px-3 py-3 text-sm font-semibold transition ${isActive ? 'bg-[#f7f1e5] text-[#173c36]' : 'text-[#c5d7ca] hover:bg-[#2a554d]'}`}><LayoutDashboard size={18} /> Notlarım</NavLink>
          <NavLink to="/notes/new" onClick={() => setMobileMenu(false)} className="flex items-center gap-3 px-3 py-3 text-sm font-semibold text-[#c5d7ca] transition hover:bg-[#2a554d]"><Plus size={18} /> Yeni not</NavLink>
        </nav>
        <div className="mt-auto border-t border-[#41655d] pt-5">
          <button onClick={logout} className="flex w-full items-center gap-3 px-3 py-3 text-sm font-semibold text-[#c5d7ca] hover:bg-[#2a554d]"><LogOut size={18} /> Çıkış yap</button>
        </div>
      </aside>
      <main className="min-w-0 flex-1">
        <header className="flex items-center justify-between border-b border-[#d9d1c3] bg-[#f5f1e9]/90 px-5 py-4 backdrop-blur lg:hidden">
          <button onClick={() => setMobileMenu(true)} aria-label="Menüyü aç"><Menu /></button>
          <Link to="/notes" className="font-bold tracking-tight">AI Notes</Link>
          <span className="h-2 w-2 bg-[#e9bb4b]" />
        </header>
        <Outlet />
      </main>
    </div>
  )
}

function AuthPage({ mode }: { mode: 'login' | 'register' }) {
  const isRegister = mode === 'register'
  const navigate = useNavigate()
  const { login, register } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setLoading(true)
    try {
      if (isRegister) await register(email, password)
      await login(email, password)
      navigate('/notes')
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'İşlem tamamlanamadı.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="noise flex min-h-screen items-center justify-center bg-[#173c36] px-5 py-10 text-[#173c36]">
      <div className="absolute left-8 top-8 text-[#f7f1e5] lg:left-14 lg:top-12"><Sparkles size={24} className="text-[#e9bb4b]" /></div>
      <div className="grid w-full max-w-5xl overflow-hidden bg-[#f7f1e5] shadow-2xl lg:grid-cols-[.8fr_1.2fr]">
        <div className="hidden flex-col justify-between bg-[#e9bb4b] p-10 lg:flex">
          <div className="font-mono text-xs uppercase tracking-[.22em]">01 / capture</div>
          <div><p className="mb-4 font-mono text-xs uppercase tracking-[.2em]">Düşüncelerini sakla.</p><h1 className="max-w-xs text-5xl font-extrabold leading-[.94] tracking-[-.06em]">Fikirler, unutulmadan.</h1></div>
          <p className="max-w-xs text-sm leading-6">Notlarını düzenle, yapay zekâ ile özünü yakala ve bir sonraki fikrine alan aç.</p>
        </div>
        <div className="p-7 sm:p-12 lg:p-16">
          <div className="mb-12 flex items-center justify-between"><span className="font-mono text-xs uppercase tracking-[.2em] text-[#658078]">AI NOTES / {isRegister ? 'JOIN' : 'WELCOME BACK'}</span><span className="h-2 w-2 bg-[#e9bb4b]" /></div>
          <h2 className="text-3xl font-extrabold tracking-[-.04em]">{isRegister ? 'Kendi arşivini aç.' : 'Devam etmeye hazır mısın?'}</h2>
          <p className="mt-3 text-sm leading-6 text-[#65736e]">{isRegister ? 'Bir hesap oluştur, düşüncelerini tek yerde tut.' : 'Notlarına dönmek için giriş yap.'}</p>
          <form onSubmit={submit} className="mt-9 space-y-5">
            <label className="block"><span className="mb-2 block font-mono text-[11px] uppercase tracking-[.14em] text-[#65736e]">Email</span><input required type="email" value={email} onChange={event => setEmail(event.target.value)} className="w-full border-b border-[#b9beb4] bg-transparent px-0 py-3 outline-none transition placeholder:text-[#9da79e] focus:border-[#173c36]" placeholder="sen@example.com" /></label>
            <label className="block"><span className="mb-2 block font-mono text-[11px] uppercase tracking-[.14em] text-[#65736e]">Şifre</span><input required minLength={8} type="password" value={password} onChange={event => setPassword(event.target.value)} className="w-full border-b border-[#b9beb4] bg-transparent px-0 py-3 outline-none transition placeholder:text-[#9da79e] focus:border-[#173c36]" placeholder="En az 8 karakter" /></label>
            {error && <p className="border-l-2 border-[#c9573d] bg-[#f5dfd6] px-3 py-2 text-sm text-[#8b3021]">{error}</p>}
            <button disabled={loading} className="flex w-full items-center justify-center gap-3 bg-[#173c36] px-5 py-4 text-sm font-bold text-[#f7f1e5] transition hover:bg-[#2b5a50] disabled:cursor-wait disabled:opacity-60">{loading && <Spinner small />}{isRegister ? 'Hesap oluştur' : 'Giriş yap'} <ArrowRight size={17} /></button>
          </form>
          <p className="mt-8 text-center text-sm text-[#65736e]">{isRegister ? 'Zaten hesabın var mı?' : 'İlk kez mi geliyorsun?'} <Link className="font-bold text-[#173c36] underline underline-offset-4" to={isRegister ? '/login' : '/register'}>{isRegister ? 'Giriş yap' : 'Kayıt ol'}</Link></p>
        </div>
      </div>
    </main>
  )
}

function NotesPage() {
  const navigate = useNavigate()
  const [notes, setNotes] = useState<Note[]>([])
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    setLoading(true)
    api.listNotes(search, page).then(response => {
      if (!active) return
      setNotes(response.items)
      setTotalPages(Math.max(response.totalPages, 1))
      setTotalCount(response.totalCount)
    }).catch(caught => active && setError(caught instanceof Error ? caught.message : 'Notlar yüklenemedi.')).finally(() => active && setLoading(false))
    return () => { active = false }
  }, [page, search])

  function submitSearch(event: FormEvent) {
    event.preventDefault()
    setPage(1)
    setSearch(searchInput)
  }

  return (
    <section className="mx-auto max-w-6xl px-5 py-8 sm:px-8 sm:py-12 lg:px-12">
      <div className="fade-up flex flex-col justify-between gap-6 border-b border-[#cfc8bb] pb-8 sm:flex-row sm:items-end"><div><p className="font-mono text-[11px] uppercase tracking-[.22em] text-[#80918a]">YOUR LIBRARY / {totalCount} NOTES</p><h1 className="mt-3 text-4xl font-extrabold tracking-[-.06em] sm:text-5xl">Notların.</h1></div><Link to="/notes/new" className="flex w-fit items-center gap-2 bg-[#e9bb4b] px-5 py-3 text-sm font-bold text-[#173c36] transition hover:bg-[#dcae3d]"><Plus size={18} /> Yeni not</Link></div>
      <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between"><form onSubmit={submitSearch} className="flex max-w-xl flex-1 items-center border-b border-[#aeb9af] pb-2"><Search size={19} className="mr-3 text-[#779087]" /><input value={searchInput} onChange={event => setSearchInput(event.target.value)} className="min-w-0 flex-1 bg-transparent text-sm outline-none placeholder:text-[#91a098]" placeholder="Başlık veya içerikte ara..." /><button className="font-mono text-[10px] uppercase tracking-[.14em] text-[#527268] hover:text-[#173c36]">Ara</button></form><div className="font-mono text-[11px] uppercase tracking-[.14em] text-[#80918a]">Sayfa {page} / {totalPages}</div></div>
      {error && <div className="mt-7 border-l-2 border-[#c9573d] bg-[#f5dfd6] px-4 py-3 text-sm text-[#8b3021]">{error}</div>}
      {loading ? <div className="grid min-h-64 place-items-center"><Spinner /></div> : notes.length === 0 ? <div className="fade-up mt-12 border border-dashed border-[#b8c1b7] p-12 text-center"><FileText className="mx-auto text-[#9aa89e]" size={32} /><h2 className="mt-4 font-bold">Henüz burada bir şey yok.</h2><p className="mt-2 text-sm text-[#718079]">İlk notunu oluştur ve düşünceni yakala.</p></div> : <div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{notes.map((note, index) => <NoteCard key={note.id} note={note} index={index} onClick={() => navigate(`/notes/${note.id}`)} />)}</div>}
      <div className="mt-10 flex items-center justify-between border-t border-[#cfc8bb] pt-5"><span className="font-mono text-[10px] uppercase tracking-[.14em] text-[#80918a]">{totalCount} kayıt</span><div className="flex gap-2"><button disabled={page <= 1} onClick={() => setPage(value => value - 1)} className="grid h-9 w-9 place-items-center border border-[#c2cabe] disabled:opacity-30"><ChevronLeft size={17} /></button><button disabled={page >= totalPages} onClick={() => setPage(value => value + 1)} className="grid h-9 w-9 place-items-center border border-[#c2cabe] disabled:opacity-30"><ChevronRight size={17} /></button></div></div>
    </section>
  )
}

function NoteCard({ note, index, onClick }: { note: Note; index: number; onClick: () => void }) {
  return <button onClick={onClick} style={{ animationDelay: `${index * 55}ms` }} className="fade-up group flex min-h-56 flex-col justify-between border border-[#d3cdc0] bg-[#faf7f0] p-5 text-left transition hover:-translate-y-1 hover:border-[#173c36] hover:shadow-[5px_5px_0_#e9bb4b]"><div><div className="flex items-start justify-between gap-4"><span className="font-mono text-[10px] uppercase tracking-[.14em] text-[#819188]">{new Date(note.updatedAt).toLocaleDateString('tr-TR')}</span><ArrowUpRightIcon /></div><h2 className="mt-7 line-clamp-2 text-xl font-extrabold leading-tight tracking-[-.035em]">{note.title}</h2><p className="mt-3 line-clamp-3 text-sm leading-6 text-[#65736e]">{note.summary || note.content}</p></div><div className="mt-6 flex flex-wrap gap-2">{note.tags.slice(0, 3).map(tag => <span key={tag} className="flex items-center gap-1 bg-[#e9eee7] px-2 py-1 font-mono text-[10px] text-[#527268]"><Hash size={10} />{tag}</span>)}</div></button>
}
function ArrowUpRightIcon() { return <ArrowRight size={17} className="-rotate-45 text-[#819188] transition group-hover:rotate-0 group-hover:text-[#173c36]" /> }

function EditorPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [note, setNote] = useState<Note | null>(null)
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [loading, setLoading] = useState(Boolean(id))
  const [saving, setSaving] = useState(false)
  const [aiAction, setAiAction] = useState<'summary' | 'tags' | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    api.getNote(id).then(response => { setNote(response); setTitle(response.title); setContent(response.content) }).catch(caught => setError(caught instanceof Error ? caught.message : 'Not bulunamadı.')).finally(() => setLoading(false))
  }, [id])

  async function save(event: FormEvent) {
    event.preventDefault()
    setSaving(true); setError('')
    try {
      const response = id ? await api.updateNote(id, title, content) : await api.createNote(title, content)
      setNote(response); setTitle(response.title); setContent(response.content)
      if (!id) navigate(`/notes/${response.id}`, { replace: true })
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'Not kaydedilemedi.') } finally { setSaving(false) }
  }

  async function summarize() {
    if (!id) return
    setAiAction('summary'); setError('')
    try { setNote(await api.summarize(id)) } catch (caught) { setError(caught instanceof Error ? caught.message : 'Özet oluşturulamadı.') } finally { setAiAction(null) }
  }
  async function suggestTags() {
    if (!id) return
    setAiAction('tags'); setError('')
    try { const response = await api.suggestTags(id); setNote(previous => previous ? { ...previous, tags: response.tags } : previous) } catch (caught) { setError(caught instanceof Error ? caught.message : 'Etiket önerilemedi.') } finally { setAiAction(null) }
  }
  async function remove() {
    if (!id || !window.confirm('Bu not silinsin mi?')) return
    await api.deleteNote(id); navigate('/notes')
  }

  if (loading) return <div className="grid min-h-screen place-items-center"><Spinner /></div>
  return <section className="mx-auto max-w-5xl px-5 py-8 sm:px-8 sm:py-12 lg:px-12"><Link to="/notes" className="inline-flex items-center gap-2 font-mono text-[11px] uppercase tracking-[.16em] text-[#6d8178] hover:text-[#173c36]"><ArrowLeft size={15} /> Notlara dön</Link><div className="mt-9 flex flex-col justify-between gap-5 border-b border-[#cfc8bb] pb-7 sm:flex-row sm:items-end"><div><p className="font-mono text-[11px] uppercase tracking-[.2em] text-[#80918a]">{id ? 'EDIT NOTE' : 'NEW NOTE'}</p><h1 className="mt-2 text-3xl font-extrabold tracking-[-.05em]">{id ? 'Notunu geliştir.' : 'Yeni bir düşünce.'}</h1></div>{id && <button onClick={remove} className="flex items-center gap-2 text-sm font-semibold text-[#a54b3a] hover:text-[#7e2d20]"><Trash2 size={16} /> Sil</button>}</div>{error && <div className="mt-6 border-l-2 border-[#c9573d] bg-[#f5dfd6] px-4 py-3 text-sm text-[#8b3021]">{error}</div>}<form onSubmit={save} className="mt-8"><input required maxLength={200} value={title} onChange={event => setTitle(event.target.value)} placeholder="Not başlığı" className="w-full bg-transparent text-4xl font-extrabold tracking-[-.06em] outline-none placeholder:text-[#a6afa5] sm:text-5xl" /><textarea required value={content} onChange={event => setContent(event.target.value)} placeholder="Aklındaki şeyi buraya bırak..." className="mt-8 min-h-[340px] w-full resize-y bg-transparent text-base leading-8 text-[#40554d] outline-none placeholder:text-[#a6afa5]" /><div className="flex flex-col justify-between gap-4 border-t border-[#cfc8bb] pt-5 sm:flex-row sm:items-center"><span className="font-mono text-[10px] uppercase tracking-[.14em] text-[#8a988f]">{content.length} karakter</span><button disabled={saving} className="flex items-center justify-center gap-2 bg-[#173c36] px-6 py-3 text-sm font-bold text-[#f7f1e5] hover:bg-[#2b5a50] disabled:opacity-60">{saving && <Spinner small />}{id ? 'Değişiklikleri kaydet' : 'Notu kaydet'} <Check size={16} /></button></div></form>{id && <div className="mt-12 grid gap-4 border-t border-[#cfc8bb] pt-8 md:grid-cols-2"><AiAction title="Özet" icon={<Sparkles size={18} />} text={note?.summary || 'Notunun ana fikrini birkaç cümlede çıkar.'} button="Özetle" loading={aiAction === 'summary'} onClick={summarize} /><AiAction title="Etiketler" icon={<Tag size={18} />} text={note?.tags.length ? note.tags.join(' · ') : 'Notunu sınıflandırmak için etiketler öner.'} button="Etiket öner" loading={aiAction === 'tags'} onClick={suggestTags} /></div>}</section>
}

function AiAction({ title, icon, text, button, loading, onClick }: { title: string; icon: ReactNode; text: string; button: string; loading: boolean; onClick: () => void }) {
  return <div className="border border-[#d3cdc0] bg-[#faf7f0] p-5"><div className="flex items-center gap-2 font-mono text-[11px] uppercase tracking-[.16em] text-[#527268]">{icon}{title}</div><p className="mt-4 min-h-14 text-sm leading-6 text-[#65736e]">{text}</p><button disabled={loading} onClick={onClick} className="mt-5 flex items-center gap-2 border border-[#9eb0a5] px-4 py-2 text-sm font-bold text-[#173c36] hover:border-[#173c36] disabled:opacity-50">{loading && <Spinner small />}{button}<Sparkles size={14} /></button></div>
}

export default App
