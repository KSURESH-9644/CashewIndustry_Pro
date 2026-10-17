import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export interface UserProfile {
  id: number;
  fullName: string;
  phoneNumber: string;
  email: string | null;
  role: string;
  roleName: string;
  preferredLanguage: string;
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('auth_token'));
  const user = ref<UserProfile | null>(
    localStorage.getItem('auth_user') ? JSON.parse(localStorage.getItem('auth_user')!) : null
  );

  const isAuthenticated = computed(() => !!token.value);
  const userRole = computed(() => user.value?.role || '');

  const setAuth = (newToken: string, newUser: UserProfile) => {
    token.value = newToken;
    user.value = newUser;
    localStorage.setItem('auth_token', newToken);
    localStorage.setItem('auth_user', JSON.stringify(newUser));
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
  };

  return {
    token,
    user,
    isAuthenticated,
    userRole,
    setAuth,
    logout,
  };
});