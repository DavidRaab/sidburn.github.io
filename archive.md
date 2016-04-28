---
layout: archive
title: Archive
---

{% for post in site.posts %}
  <div class="post-archive">
    <a href="{{ post.url | date_to_long_string }}">{{ post.title }}</a>
    <div class="post-header">
      <span class="post-date"></span>
      {% include post-tags.html %}
    </div>
  </div>
{% endfor %}