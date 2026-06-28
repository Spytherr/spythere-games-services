export interface Game {
  Id: number
  Key: string
  Name: string
  Description: string
}

export interface LeaderboardEntry {
  Rank: number
  DisplayName: string
  ScoreValue: number
  Platform: string
}