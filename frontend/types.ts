export interface Game {
  Id: number
  Name: string
  Description: string
}

export interface LeaderboardEntry {
  Rank: number
  DisplayName: string
  ScoreValue: number
  Platform: string
}