import Hero from './components/Hero'
import GamesSection from './components/GameSection'
import LeaderboardSection from './components/LeaderboardSection'
import ThemeToggle from './components/ThemeToggle'
import PixelBackground from './components/PixelBackground'

function App() {
  return (
    <div className="min-h-screen relative">
      <PixelBackground />
      <div className="relative z-10">
        <ThemeToggle />
        <Hero />
        <GamesSection />
        <LeaderboardSection />
      </div>
    </div>
  )
}

export default App