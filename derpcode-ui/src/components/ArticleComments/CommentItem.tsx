import { useState } from 'react';
import { Card, CardBody, Button, Textarea, Avatar } from '@heroui/react';
import {
  ChevronUpIcon,
  ChevronDownIcon,
  ArrowUturnLeftIcon,
  ChatBubbleBottomCenterTextIcon
} from '@heroicons/react/24/outline';
import type { ArticleComment } from '../../types/models';
import type { User } from '../../types/auth';
import { useArticleCommentReplies } from '../../hooks/api';
import { MarkdownRenderer } from '../MarkdownRenderer';

const formatTimeAgo = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInMs = now.getTime() - date.getTime();
  const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
  const diffInHours = Math.floor(diffInMinutes / 60);
  const diffInDays = Math.floor(diffInHours / 24);

  if (diffInMinutes < 1) return 'just now';
  if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
  if (diffInHours < 24) return `${diffInHours}h ago`;
  if (diffInDays < 30) return `${diffInDays}d ago`;

  return date.toLocaleDateString();
};

interface CommentItemProps {
  comment: ArticleComment;
  level: number;
  onReply: (commentId: number, content: string, quotedCommentId?: number) => Promise<void>;
  isReplying: boolean;
  user: User | null;
  articleId: number;
  quotedComments: Map<number, ArticleComment>;
  onLoadReplies?: () => void;
}

export const CommentItem = ({
  comment,
  level,
  onReply,
  isReplying,
  user,
  articleId,
  quotedComments,
  onLoadReplies
}: CommentItemProps) => {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const [replyContent, setReplyContent] = useState('');
  const [showReplies, setShowReplies] = useState(false);

  const quotedComment = comment.quotedCommentId ? quotedComments.get(comment.quotedCommentId) : undefined;

  // Use React Query to fetch replies, but only when we want to show them
  const {
    data: repliesResponse,
    isLoading: loadingReplies,
    refetch: refetchReplies
  } = useArticleCommentReplies(
    articleId,
    comment.id,
    {
      first: 50,
      includeNodes: true
    },
    {
      enabled: showReplies && level === 0 // Only fetch when showing replies and for top-level comments
    }
  );

  const replies = repliesResponse?.nodes || [];

  const loadReplies = () => {
    if (level > 0) return; // Only load replies for top-level comments
    setShowReplies(true);
  };

  const handleReply = async (asQuote = false) => {
    if (!replyContent.trim()) return;

    try {
      await onReply(
        level === 0 ? comment.id : comment.parentCommentId || comment.id,
        replyContent,
        asQuote ? comment.id : undefined
      );
      setReplyContent('');
      setShowReplyForm(false);

      // If this is a top-level comment and we're showing replies, refetch them
      if (level === 0 && showReplies) {
        refetchReplies();
      } else if (onLoadReplies) {
        onLoadReplies();
      }
    } catch (error) {
      console.error('Failed to submit reply:', error);
    }
  };

  const handleQuote = () => {
    const quotedText = `> ${comment.content}\n\n`;
    setReplyContent(quotedText);
    setShowReplyForm(true);
  };

  return (
    <div className={`${level > 0 ? 'ml-8 border-l-2 border-default-200 pl-4' : ''}`}>
      <Card className="mb-4">
        <CardBody className="p-4">
          {quotedComment && (
            <div className="mb-3 p-3 bg-default-50 dark:bg-default-800/50 rounded-lg border-l-4 border-primary">
              <div className="flex items-center gap-2 mb-2">
                <ChatBubbleBottomCenterTextIcon className="w-4 h-4 text-default-500" />
                <span className="text-sm text-default-500">Replying to {quotedComment.userName}</span>
              </div>
              <p className="text-sm text-default-600 italic">
                {quotedComment.content.length > 100
                  ? `${quotedComment.content.substring(0, 100)}...`
                  : quotedComment.content}
              </p>
            </div>
          )}

          <div className="flex items-start gap-3">
            <Avatar size="sm" name={comment.userName} className="flex-shrink-0" />
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <span className="font-semibold text-sm">{comment.userName}</span>
                <span className="text-xs text-default-500">{formatTimeAgo(comment.createdAt)}</span>
                {comment.isEdited && <span className="text-xs text-default-400">(edited)</span>}
              </div>

              <div className="prose prose-sm dark:prose-invert max-w-none">
                <MarkdownRenderer content={comment.content} />
              </div>

              <div className="flex items-center gap-4">
                <div className="flex items-center gap-1">
                  <Button size="sm" variant="ghost" isIconOnly className="min-w-unit-8 w-8 h-8">
                    <ChevronUpIcon className="w-4 h-4" />
                  </Button>
                  <span className="text-sm text-default-600">{comment.upVotes - comment.downVotes}</span>
                  <Button size="sm" variant="ghost" isIconOnly className="min-w-unit-8 w-8 h-8">
                    <ChevronDownIcon className="w-4 h-4" />
                  </Button>
                </div>

                {user && (
                  <div className="flex items-center gap-2">
                    <Button
                      size="sm"
                      variant="ghost"
                      startContent={<ArrowUturnLeftIcon className="w-4 h-4" />}
                      onPress={() => {
                        if (level === 1) {
                          handleQuote();
                        } else {
                          setShowReplyForm(!showReplyForm);
                        }
                      }}
                      className="text-xs"
                    >
                      Reply
                    </Button>
                  </div>
                )}

                {/* Show/Hide Replies Button for top-level comments */}
                {level === 0 && comment.repliesCount > 0 && (
                  <Button
                    size="sm"
                    variant="ghost"
                    onPress={() => {
                      if (showReplies) {
                        setShowReplies(false);
                      } else {
                        loadReplies();
                      }
                    }}
                    isLoading={loadingReplies}
                    className="text-xs"
                  >
                    {showReplies
                      ? 'Hide replies'
                      : `Show ${comment.repliesCount} ${comment.repliesCount === 1 ? 'reply' : 'replies'}`}
                  </Button>
                )}
              </div>
            </div>
          </div>

          {showReplyForm && user && (
            <div className="mt-4 pl-11">
              <Textarea
                placeholder={level === 1 ? 'Write a quoted reply...' : 'Write a reply...'}
                value={replyContent}
                onValueChange={setReplyContent}
                minRows={3}
                maxRows={8}
              />
              <div className="flex justify-end gap-2 mt-2">
                <Button
                  size="sm"
                  variant="ghost"
                  onPress={() => {
                    setShowReplyForm(false);
                    setReplyContent('');
                  }}
                >
                  Cancel
                </Button>
                <Button
                  size="sm"
                  color="primary"
                  onPress={() => handleReply(level === 1)}
                  isLoading={isReplying}
                  isDisabled={!replyContent.trim()}
                >
                  Reply
                </Button>
              </div>
            </div>
          )}
        </CardBody>
      </Card>

      {/* Render replies for top-level comments */}
      {level === 0 && showReplies && replies.length > 0 && (
        <div className="ml-4">
          {replies.map(reply => (
            <CommentItem
              key={reply.id}
              comment={reply}
              level={1}
              onReply={onReply}
              isReplying={isReplying}
              user={user}
              articleId={articleId}
              quotedComments={quotedComments}
              onLoadReplies={loadReplies}
            />
          ))}
        </div>
      )}
    </div>
  );
};
