export function getGameDescription(gameKey: string): string {
  switch (gameKey) {
    case 'chess-vs-checkers':
      return 'Chess vs Checkers is a roguelike strategy game blending chess-inspired abilities with grid-based tactical gameplay. Survive escalating waves of enemy checkers and spend rewards in the shop to upgrade your build.';
    default:
      return 'No description available for this game.';
  }
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