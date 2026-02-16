#!/bin/sh
set -e

BACKUP_ROOT=/srv/backups/wimpeytrack
DATE=$(date +%F)

echo "[$(date)] Starting backup"

mkdir -p "$BACKUP_ROOT/$DATE"

# Docker volumes
for VOLUME in wimpeytrack-db wimpeytrack-uploads wimpeytrack-reports; do
  echo "Backing up volume: $VOLUME"
  
  /usr/bin/docker run --rm \
    -v ${VOLUME}:/volume:ro \
    -v "$BACKUP_ROOT/$DATE":/backup \
    alpine \
    tar czf /backup/${VOLUME}.tar.gz -C /volume .
done

# Keep of 14 days
find "$BACKUP_ROOT" -mindepth 1 -maxdepth 1 -type d -mtime +14 -exec rm -rf {} \;

echo "[$(date)] Backup completed"
