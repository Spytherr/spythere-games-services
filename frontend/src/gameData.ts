import type { Game } from './types'

export const STATIC_GAMES: Game[] = [
  { Id: -1, Key: 'globetry', Name: 'Globetry', Description: '' },
]

export function getGameDescription(gameKey: string): string {
  switch (gameKey) {
    case 'chess-vs-checkers':
      return 'Chess vs Checkers is a roguelike strategy game blending chess-inspired abilities with grid-based tactical gameplay. Survive escalating waves of enemy checkers and spend rewards in the shop to upgrade your build.';
    case 'globetry':
      return 'Globetry is a daily geography puzzle played in the browser. Find the hidden country by comparing chosen metrics with each guess on an interactive 3D globe. A new country every day!';
    default:
      return 'No description available for this game.';
  }
}

export function getGamePlayUrl(gameKey: string): string | null {
  if (gameKey === 'globetry') return 'https://globetry.vercel.app/'

  return null
}

export function getPlatformImage(platform: string): string {
  switch (platform) {
    case 'android':
      return '/android.png';
    case 'ios':
      return '/ios.png';
    default:
      return '/default-platform.png';
  }
}