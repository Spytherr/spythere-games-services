import type { Game, LeaderboardEntry } from '../types'
 
const API_URL = import.meta.env.VITE_API_URL
 
export async function fetchGames(): Promise<Game[]> {
  const res = await fetch(`${API_URL}/api/games`)
  if (!res.ok) throw new Error('Failed to fetch games')
  return res.json()
}
 
export async function fetchTopScores(gameKey: string, count = 10): Promise<LeaderboardEntry[]> {
  const res = await fetch(`${API_URL}/api/games/${gameKey}/scores/top?count=${count}`)
  if (!res.ok) throw new Error('Failed to fetch leaderboard')
  return res.json()
}