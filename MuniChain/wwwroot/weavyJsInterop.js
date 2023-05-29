export function weavy(url, token, timezone) {
    return new Weavy({
        url: url,
        tz: timezone,
        tokenFactory: async (refresh) => {
            return token;
        }
    });
}