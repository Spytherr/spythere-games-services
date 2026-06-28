import { useState, useEffect } from 'react'
import type { Game, LeaderboardEntry } from '../types'
import { fetchGames, fetchTopScores } from '../api/client'

function LeaderboardSection() {
  const [games, setGames] = useState<Game[]>([])
  const [selectedGame, setSelectedGame] = useState<string | null>(null)
  const [scores, setScores] = useState<LeaderboardEntry[]>([])
  const [loading, setLoading] = useState(false)

  // 1) Załaduj listę gier raz przy starcie
  useEffect(() => {
    fetchGames()
      .then(setGames)
      .catch(console.error)
  }, [])

  // 2) Załaduj scores gdy zmieni się wybrana gra
  useEffect(() => {
    if (!selectedGame) return
    setLoading(true)
    fetchTopScores(selectedGame)
      .then(setScores)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [selectedGame])

  // 3) Auto-wybierz pierwszą grę gdy lista się załaduje
  useEffect(() => {
    if (games.length > 0 && !selectedGame) {
      setSelectedGame(games[0].Key)
    }
  }, [games])

  return (
    <section className="py-16 px-4">
      <h2 className="text-3xl font-bold text-center mb-8">Leaderboards</h2>

      {/* Tabs do wyboru gry */}
      <div className="flex justify-center gap-2 mb-8 flex-wrap">
        {games.map((game) => (
          <button
            key={game.Id}
            onClick={() => setSelectedGame(game.Key)}
            className={`px-4 py-2 rounded-lg transition ${
              selectedGame === game.Key
                ? 'bg-purple-600 text-white'
                : 'bg-gray-200 hover:bg-gray-300'
            }`}
          >
            {game.Name}
          </button>
        ))}
      </div>

      {/* Tabela wyników */}
      {loading ? (
        <p className="text-center">Loading...</p>
      ) : (
        <table className="mx-auto max-w-2xl w-full border-collapse">
          <thead>
            <tr className="border-b border-gray-300">
              <th className="py-2 px-4 text-left">#</th>
              <th className="py-2 px-4 text-left">Player</th>
              <th className="py-2 px-4 text-right">Score</th>
              <th className="py-2 px-4 text-left">Platform</th>
            </tr>
          </thead>
          <tbody>
            {scores.map((entry) => (
              <tr key={entry.Rank} className="border-b border-gray-100">
                <td className="py-2 px-4">{entry.Rank}</td>
                <td className="py-2 px-4">{entry.DisplayName}</td>
                <td className="py-2 px-4 text-right">{entry.ScoreValue}</td>
                <td className="py-2 px-4">{entry.Platform}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}

export default LeaderboardSection