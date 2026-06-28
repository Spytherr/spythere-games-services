import Hero from './components/Hero'
import GamesSection from './components/GameSection'
import LeaderboardSection from './components/LeaderboardSection'

function App() {
  return (
    <div className="min-h-screen">
      <Hero />
      <GamesSection />
      <LeaderboardSection />
    </div>
  )
}

export default App