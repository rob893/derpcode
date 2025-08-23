import { useState, useMemo } from 'react';
import { Card, CardBody, Button, Textarea, Divider, Spinner } from '@heroui/react';
import { ChatBubbleLeftIcon } from '@heroicons/react/24/outline';
import type { User } from '../../types/auth';
import { useArticleComments, useCreateArticleComment } from '../../hooks/api';
import { CommentItem } from './CommentItem';
import { ArticleCommentOrderBy, OrderByDirection } from '../../types/models';

interface ArticleCommentsProps {
  articleId: number;
  user: User | null;
}

export const ArticleComments = ({ articleId, user }: ArticleCommentsProps) => {
  const [newComment, setNewComment] = useState('');
  const [showCommentForm, setShowCommentForm] = useState(false);

  // Use React Query hooks
  const {
    data: commentsResponse,
    isLoading,
    error
  } = useArticleComments(articleId, {
    first: 50,
    orderBy: ArticleCommentOrderBy.MostRecent,
    orderByDirection: OrderByDirection.Descending,
    includeNodes: true
  });

  const createCommentMutation = useCreateArticleComment(articleId);

  // Process the comments data
  const { topLevelComments, quotedComments } = useMemo(() => {
    const allComments = commentsResponse?.nodes || [];

    // Filter to only show top-level comments (no parentCommentId)
    const topLevel = allComments.filter(comment => !comment.parentCommentId);

    // Create a map of all comments for quoted comment lookup
    const commentMap = new Map();
    allComments.forEach(comment => {
      commentMap.set(comment.id, comment);
    });

    return {
      topLevelComments: topLevel,
      quotedComments: commentMap
    };
  }, [commentsResponse]);

  const handleSubmitComment = async () => {
    if (!newComment.trim() || !user) return;

    try {
      await createCommentMutation.mutateAsync({
        content: newComment.trim()
      });
      setNewComment('');
      setShowCommentForm(false);
    } catch (error) {
      console.error('Failed to submit comment:', error);
    }
  };

  const handleReply = async (parentCommentId: number, content: string, quotedCommentId?: number) => {
    if (!user) return;

    try {
      await createCommentMutation.mutateAsync({
        content: content.trim(),
        parentCommentId,
        quotedCommentId
      });
    } catch (error) {
      console.error('Failed to submit reply:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-danger">Failed to load comments</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h3 className="text-xl font-semibold flex items-center gap-2">
          <ChatBubbleLeftIcon className="w-5 h-5" />
          Discussion ({topLevelComments.length})
        </h3>
        {user && (
          <Button color="primary" variant="ghost" onPress={() => setShowCommentForm(!showCommentForm)}>
            Add Comment
          </Button>
        )}
      </div>

      {showCommentForm && user && (
        <Card>
          <CardBody className="p-4">
            <Textarea
              placeholder="Share your thoughts about this explanation..."
              value={newComment}
              onValueChange={setNewComment}
              minRows={4}
              maxRows={8}
            />
            <div className="flex justify-end gap-2 mt-3">
              <Button
                variant="ghost"
                onPress={() => {
                  setShowCommentForm(false);
                  setNewComment('');
                }}
              >
                Cancel
              </Button>
              <Button
                color="primary"
                onPress={handleSubmitComment}
                isLoading={createCommentMutation.isPending}
                isDisabled={!newComment.trim()}
              >
                Post Comment
              </Button>
            </div>
          </CardBody>
        </Card>
      )}

      <Divider />

      {topLevelComments.length === 0 ? (
        <div className="text-center py-12">
          <ChatBubbleLeftIcon className="w-12 h-12 mx-auto text-default-300 mb-4" />
          <p className="text-default-500 text-lg">No comments yet</p>
          <p className="text-default-400 text-sm mt-2">Be the first to share your thoughts about this explanation!</p>
        </div>
      ) : (
        <div className="space-y-4">
          {topLevelComments.map(comment => (
            <CommentItem
              key={comment.id}
              comment={comment}
              level={0}
              onReply={handleReply}
              isReplying={createCommentMutation.isPending}
              user={user}
              articleId={articleId}
              quotedComments={quotedComments}
            />
          ))}
        </div>
      )}
    </div>
  );
};
