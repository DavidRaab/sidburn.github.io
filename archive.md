---
layout: archive
title: Archive
---

{% for post in site.posts %}
  <div class="post-archive">
    <a class="post-title" href="{{ post.url }}">{{ post.title }}</a>
    <div class="post-header">
      <span class="post-date">{{ post.url | date_to_long_string }}</span>
      {% include post-tags.html %}
    </div>
  </div>
{% endfor %}