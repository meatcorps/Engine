SRC="Assets/SoundFX"
DST="Build/SoundsWav"

find "$SRC" -type f \( -iname '*.wav' -o -iname '*.mp3' -o -iname '*.ogg' -o -iname '*.flac' \) -print0 \
| while IFS= read -r -d '' f; do
  rel="${f#$SRC/}"
  out="$DST/${rel%.*}.wav"
  mkdir -p "$(dirname "$out")"
  ffmpeg -y -i "$f" -map_metadata -1 -ac 1 -ar 44100 -c:a pcm_s16le "$out"
done