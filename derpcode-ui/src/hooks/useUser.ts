import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { userApi } from '../services/user';
import { useAuth } from './useAuth';

export function useUser(userId?: number) {
  return useQuery({
    queryKey: ['user', userId],
    queryFn: () => userApi.getUserById(userId!),
    enabled: !!userId
  });
}

export function useCurrentUser() {
  const { user } = useAuth();
  return useUser(user?.id);
}

export function useDeleteUser() {
  const queryClient = useQueryClient();
  const { logout } = useAuth();

  return useMutation({
    mutationFn: userApi.deleteUser,
    onSuccess: () => {
      // Clear all user-related queries
      queryClient.invalidateQueries({ queryKey: ['user'] });
      // Log out the user since their account no longer exists
      logout();
    }
  });
}

export function useDeleteLinkedAccount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, linkedAccountType }: { userId: number; linkedAccountType: string }) =>
      userApi.deleteLinkedAccount(userId, linkedAccountType),
    onSuccess: () => {
      // Invalidate and refetch user data to update the UI
      queryClient.invalidateQueries({ queryKey: ['user'] });
    }
  });
}
