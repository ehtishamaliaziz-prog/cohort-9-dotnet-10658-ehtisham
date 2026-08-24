// In-memory token store. Not persisted to localStorage/sessionStorage,
// so it's cleared on page refresh (mitigates XSS token theft) and is
// readable by both the React tree (via AuthContext) and the axios
// client (which isn't part of the React tree).
let currentToken = null;

export function getToken() {
  return currentToken;
}

export function setToken(token) {
  currentToken = token;
}

export function clearToken() {
  currentToken = null;
}