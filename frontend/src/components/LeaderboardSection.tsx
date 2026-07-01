import { useState, useEffect } from 'react'
import type { Game, LeaderboardEntry } from '../types'
import { fetchGames, fetchTopScores } from '../api/client'
import { getPlatformImage } from '../gameData'
import { useInView } from '../hooks/useInView'

function LeaderboardSection() {
  const [games, setGames] = useState<Game[]>([])
  const [selectedGame, setSelectedGame] = useState<string | null>(null)
  const [scores, setScores] = useState<LeaderboardEntry[]>([])
  const [loading, setLoading] = useState(false)
  const { ref, inView } = useInView<HTMLElement>()

  useEffect(() => {
    fetchGames()
      .then(setGames)
      .catch(console.error)
  }, [])

  useEffect(() => {
    if (!selectedGame) return
    setLoading(true)
    fetchTopScores(selectedGame)
      .then(setScores)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [selectedGame])

  useEffect(() => {
    if (games.length > 0 && !selectedGame) {
      setSelectedGame(games[0].Key)
    }
  }, [games])

  return (
    <section ref={ref} className={`py-16 px-4 transition-all duration-500 ${
      inView ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
    }`}>
      <h2 className="text-6xl text-center mb-8">Leaderboards</h2>

      {/* Tabs do wyboru gry */}
      <div className="flex justify-center gap-2 mb-8 flex-wrap">
        {games.map((game) => (
          <button
            key={game.Id}
            onClick={() => setSelectedGame(game.Key)}
            className={`px-4 py-2 pixel-outline transition ${selectedGame === game.Key
              ? 'bg-blue-500 text-white'
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
        <table className="mx-auto max-w-2xl w-full border-collapse table-fixed pixel-outline bg-[var(--bg)]/80 backdrop-blur-sm">
          <thead>
            <tr className="border-b border-gray-300">
              <th className="py-2 px-4 text-left w-16">#</th>
              <th className="py-2 px-4 text-left">Player</th>
              <th className="py-2 px-4 text-right w-32">Score</th>
              <th className="py-2 px-4 text-center w-24">Platform</th>
            </tr>
          </thead>
          <tbody>
            {scores.map((entry) => (
              <tr key={entry.Rank} className="border-b border-gray-100">
                <td className="py-2 px-4 text-left">{entry.Rank}</td>
                <td className="py-2 px-4 text-left">{entry.DisplayName}</td>
                <td className="py-2 px-4 text-right">{entry.ScoreValue}</td>
                <td className="py-2 px-4 text-center">
                  <img
                    src={getPlatformImage(entry.Platform)}
                    alt={entry.Platform}
                    className="w-8 h-8 inline-block"
                    style={{ imageRendering: 'pixelated' }}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}

export default LeaderboardSection