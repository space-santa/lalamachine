#!/bin/bash

# List of directories
directories=(
  "LalaDb"
  "Lalamachine.Wpf.Unittests"
  "Lalamachine.Wpf"
  "LibLala.Unittests"
  "LibLala"
  "Settings.Unittests"
  "Settings"
)

# Loop through each directory and remove bin and obj folders
for dir in "${directories[@]}"; do
  if [ -d "$dir" ]; then
    echo "Removing bin and obj folders in $dir"
    rm -rf "$dir/bin" "$dir/obj"
  else
    echo "Directory $dir does not exist"
  fi
done

echo "Cleanup complete!"
