Task description: https://minio.eidoloncorp.com/custom/hr/test-assignment-unity-dev.html

Final version contains no logs, I removed them in the last commit for better readability. 
Described some of my thoughts during completing the task as comments inside the Service. Would be great to discuss them, maybe there are some easier approaches.

I decides to use double buffering pattern, but without swapping the arrays. Idea was to keep receiving the events while request is performing.
Also hope that I have taken into account all cases of lost unsent messages :)
