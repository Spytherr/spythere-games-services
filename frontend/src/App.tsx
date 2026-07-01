import { useEffect } from 'react'
import Hero from './components/Hero'
import GamesSection from './components/GameSection'
import LeaderboardSection from './components/LeaderboardSection'
import PixelBackground from './components/PixelBackground'

function App() {
  useEffect(() => {
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const root = document.documentElement
    if (mq.matches) root.classList.add('dark')
    else root.classList.remove('dark')
    const handler = (e: MediaQueryListEvent) => {
      if (e.matches) root.classList.add('dark')
      else root.classList.remove('dark')
    }
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  return (
    <div className="min-h-screen relative">
      <PixelBackground />
      <div className="relative z-10">
        <Hero />
        <GamesSection />
        <LeaderboardSection />
      </div>
    </div>
  )
}

export default App