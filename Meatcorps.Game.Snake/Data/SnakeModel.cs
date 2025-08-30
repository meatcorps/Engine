using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Data;

public class SnakeModel
{
    private readonly Texture2DItem<SnakeSprites> _sprites;
    private readonly LevelData _level;
    public Vector2 CenterPosition { get; private set; }
    
    public Rectangle HeadRenderPosition { get; private set; }
    public Rectangle HeadSprite { get; private set; }
    public float HeadRotation { get; private set; }
    
    public Rectangle TailRenderPosition { get; private set; }
    public Rectangle TailSprite { get; private set; }
    public float TailRotation { get; private set; }
    public bool HeadIsProcessing { get; private set; }
    public bool TailIsProcessing { get; private set; }
    
    public int BodyCount => _snake.Count;
    public PointInt HeadDirection => _snake.Count > 0 ? _snake[0].Direction : PointInt.Zero;

    private List<SnakeBodyListItem> _snake = new();
    public IReadOnlyList<SnakeBodyListItem> Segments => _snake;
    private List<SnakeSprites> _snakeSprites = new();
    private Queue<Rectangle> _snakeRenderSourcePosition = new();
    private Queue<Rectangle> _snakeRenderDestinationPosition = new();
    private readonly FixedTimer _animationTimer = new(250);
    private int _renderIndex;
    private List<SnakeBodyListItem> _tempSnake = new();

    public SnakeModel(Texture2DItem<SnakeSprites> sprites, LevelData level, PointInt[] startPositions, PointInt startDirection)
    {
        _sprites = sprites;
        _level = level;
        SetupPlayer(startPositions, startDirection);
    }
    
    private void SetupPlayer(PointInt[] startPositions, PointInt startDirection)
    {
        _snake.Clear();
        for (var i = 0; i < startPositions.Length; i++)
        {
            var position = startPositions[i];
            var direction = i == 0 ? startDirection : startPositions[i - 1] - position;

            _snake.Add(new SnakeBodyListItem
            {
                Position = position,
                Direction = direction,
                IsProcessing = false
            });
        }
    }

    public PointInt NextPosition(PointInt direction)
    {
        var newHead = _snake[0].Position + direction;

        // Wrap around screen
        if (newHead.X < 0) newHead.X = _level.LevelWidth - 1;
        if (newHead.Y < 0) newHead.Y = _level.LevelHeight - 1;
        if (newHead.X >= _level.LevelWidth) newHead.X = 0;
        if (newHead.Y >= _level.LevelHeight) newHead.Y = 0;
        return newHead;
    }

    public bool RemoveRandomSegment(out PointInt position)
    {
        
        _tempSnake.Clear();
        foreach (var segment in _snake)
        {
            if (segment.IsDestroyed)
                continue;
            _tempSnake.Add(segment);
        }
        if (_tempSnake.Count < 2)
        {
            position = default;
            return false;
        }
        
        var index = Raylib.GetRandomValue(0, _tempSnake.Count - 1);
        position = _tempSnake[index].Position;
        for (var i = index; i < _snake.Count - 1; i++)
        {
            var segment = _snake[i];
            if (segment.Position == position)
            {
                segment.IsDestroyed = true;
                _snake[i] = segment;
                break;
            }
        }
            
        return true;
    }

    public void Move(PointInt nextPosition, PointInt direction, bool processingConsumable)
    {
        HeadIsProcessing = processingConsumable;
        _snake.Insert(0, new SnakeBodyListItem 
        {
            Position = nextPosition,
            Direction = direction,
            IsProcessing = processingConsumable
        });
        
        TailIsProcessing = false;
        if (_snake[^1].IsProcessing)
        {
            var tail = _snake[^1];
            tail.IsProcessing = false;
            TailIsProcessing = true;
            _snake[^1] = tail;
        }
        else
            _snake.RemoveAt(_snake.Count - 1);
    }

    public void Update(float deltaTime, bool playAnimation)
    {
        if (playAnimation)
            _animationTimer.Update(deltaTime);

        var position = Vector2.Zero;
        foreach (var segment in Segments)
        {
            position += _level.ToWorldPosition(segment.Position, true);
        }
        position /= Segments.Count;
        
        CenterPosition = Tween.StepTo(CenterPosition, position, Vector2.Distance(position, CenterPosition), deltaTime);
    }

    public void StartRender(float moveTimerNormal)
    {
        SetCorrectSprites();
        SetRenderPositions(moveTimerNormal);
        _renderIndex = 0;
    }
    
    public bool TryRenderSegment(out Rectangle sourceRect, out Rectangle destRect, out SnakeBodyListItem segment, out bool isTail)
    {
        if (_snakeRenderSourcePosition.TryDequeue(out sourceRect) &&
            _snakeRenderDestinationPosition.TryDequeue(out destRect))
        {
            segment = _snake[_renderIndex];
            isTail = _renderIndex == _snake.Count - 1;
            _renderIndex++;
            return true;
        }

        segment = default;
        sourceRect = default;
        destRect = default;
        isTail = false;
        return false;
    }

    private void SetCorrectSprites()
    {
        _snakeSprites.Clear();
        
        if (_snake[0].IsLeftRight)
            _snakeSprites.Add(SnakeSprites.SnakeBodyLeftRight);
        if (_snake[0].IsUpDown)
            _snakeSprites.Add(SnakeSprites.SnakeBodyTopBottom);

        for (var i = 1; i < _snake.Count - 1; i++)
        {
            var positionFrom = _snake[i - 1].Position - _snake[i].Position;
            var positionTo = _snake[i + 1].Position - _snake[i].Position;

            if ((positionFrom.X == 1 && positionTo.Y == 1) || (positionTo.X == 1 && positionFrom.Y == 1))
            {
                _snakeSprites.Add(SnakeSprites.SnakeBodyCornerRightBottom);
                continue;
            }

            if ((positionFrom.X == -1 && positionTo.Y == 1) || (positionTo.X == -1 && positionFrom.Y == 1))
            {
                _snakeSprites.Add(SnakeSprites.SnakeBodyCornerLeftBottom);
                continue;
            }

            if ((positionFrom.X == 1 && positionTo.Y == -1) || (positionTo.X == 1 && positionFrom.Y == -1))
            {
                _snakeSprites.Add(SnakeSprites.SnakeBodyCornerRightTop);
                continue;
            }

            if ((positionFrom.X == -1 && positionTo.Y == -1) || (positionTo.X == -1 && positionFrom.Y == -1))
            {
                _snakeSprites.Add(SnakeSprites.SnakeBodyCornerLeftTop);
                continue;
            }
            
            if (_snake[i].IsLeftRight)
                _snakeSprites.Add(SnakeSprites.SnakeBodyLeftRight);
            
            if (_snake[i].IsUpDown)
                _snakeSprites.Add(SnakeSprites.SnakeBodyTopBottom);
        }

        if (_snake.Count > 2)
        {
            var positionToTail = _snake[^2].Position - _snake[^1].Position;

            if (positionToTail.Y == 0)
                _snakeSprites.Add(SnakeSprites.SnakeBodyLeftRight);
            if (positionToTail.X == 0)
                _snakeSprites.Add(SnakeSprites.SnakeBodyTopBottom);
        }
    }

    private void SetRenderPositions(float timerNormal)
    {
        _snakeRenderDestinationPosition.Clear();
        _snakeRenderSourcePosition.Clear();
        
        var count = 0;
        foreach (var segment in _snake)
        {
            var tail = count == _snake.Count - 1;
            var head = count == 0;
            var width = _level.GridSize;
            var height = _level.GridSize;
            var offsetX = 0;
            var offsetY = 0;
            
            if (head)
            {
                if (segment.IsLeftRight)
                {
                    width = (int)(_level.GridSize * timerNormal);
                }

                if (segment.IsUpDown)
                {
                    height = (int)(_level.GridSize * timerNormal);
                }

                if (segment.Direction.X == -1)
                    offsetX = _level.GridSize - (int)(_level.GridSize * timerNormal);
                if (segment.Direction.Y == -1)
                    offsetY = _level.GridSize - (int)(_level.GridSize * timerNormal);
            }   
            
            if (tail)
            {
                var secondToLast = _snake[count - 1];
                if (secondToLast.IsLeftRight)
                {
                    width = _level.GridSize - (int)(_level.GridSize * timerNormal);
                }

                if (secondToLast.IsUpDown)
                {
                    height = _level.GridSize - (int)(_level.GridSize * timerNormal);
                }

                if (secondToLast.Direction.X == 1)
                    offsetX = (int)(_level.GridSize * timerNormal);
                if (secondToLast.Direction.Y == 1)
                    offsetY = (int)(_level.GridSize * timerNormal);
            }   
            
            var sourceRect = _sprites.GetSprite(_snakeSprites[count]);
            sourceRect.Width = width;
            sourceRect.Height = height;
            sourceRect.X += offsetX;
            sourceRect.Y += offsetY;
            
            var destRect = new Rectangle((segment.X * _level.GridSize) + offsetX,
                (segment.Y * _level.GridSize) + offsetY, width, height);
            
            _snakeRenderSourcePosition.Enqueue(sourceRect);
            _snakeRenderDestinationPosition.Enqueue(destRect);
            
            count++;
        }
        
        var headSegment = _snake[0];
        var headPosition = new Vector2(headSegment.X * _level.GridSize, headSegment.Y * _level.GridSize);
        if (headSegment.IsLeftRight)
            headPosition += new Vector2((_level.GridSize * timerNormal) * headSegment.Direction.X, 0);

        if (headSegment.IsUpDown)
            headPosition += new Vector2(0, (_level.GridSize * timerNormal) * headSegment.Direction.Y);
        
        HeadSprite = _sprites.GetAnimation(SnakeSprites.SnakeHeadAnimation, (int)(_animationTimer.NormalizedElapsed * 5));
        HeadRenderPosition = new Rectangle(headPosition + new Vector2(8, 8) - headSegment.Direction.ToVector2() * 7,
            HeadSprite.Size);
        HeadRotation = GetRotation(headSegment.Direction);
        
        var tailSegment = _snake[^1];
        var previousTailSegment = _snake[^2];
        var tailPosition = new Vector2(tailSegment.X * _level.GridSize, tailSegment.Y * _level.GridSize);
        if (previousTailSegment.IsLeftRight)
            tailPosition += new Vector2((_level.GridSize * timerNormal) * previousTailSegment.Direction.X, 0);

        if (previousTailSegment.IsUpDown)
            tailPosition += new Vector2(0, (_level.GridSize * timerNormal) * previousTailSegment.Direction.Y);
        
        TailSprite = _sprites.GetSprite(SnakeSprites.SnakeTail);
        TailRenderPosition = new Rectangle(tailPosition + new Vector2(8, 8) - previousTailSegment.Direction.ToVector2() * 4, TailSprite.Size);
        TailRotation = GetRotation(previousTailSegment.Direction);
    }

    private float GetRotation(PointInt direction)
    {
        if (direction.X == 1)
            return 0;
        if (direction.X == -1)
            return 180;
        if (direction.Y == -1)
            return 270;
        if (direction.Y == 1)
            return 90;
        
        return 0;
    }
    
}

public struct SnakeBodyListItem
{
    public int X => Position.X;
    public int Y => Position.Y;
    public PointInt Position { get; set; }
    public PointInt Direction { get; set; }
    public bool IsProcessing { get; set; }
    
    public bool IsUpDown => Direction.X == 0;
    public bool IsLeftRight => Direction.Y == 0;
    public bool IsDestroyed { get; set; }
}