---
layout: archive
title: Archive
---

<div class="post-archive">
  {% for post in site.posts %}
    {% assign currentdate = post.date | date: "%Y" %}
    {% if currentdate != date %}
      <h2>{{ currentdate }}</h2>
      {% assign date = currentdate %}
    {% endif %} 
    
    <article>
      <h1><a href="{{ post.url }}">{{ post.title }}</a></h1>
      <time datetime="{{ post.date | date_to_xmlschema }}" pubdate>
        <span class="month">{{ post.date | date: "%d" }}</span>
        <span class="day">{{ post.date | date: "%b" }}</span>
        <span class="year">{{ post.date | date: "%Y" }}</span>
      </time>
      <footer>{% include post-tags.html %}</footer>
    </article>
  {% endfor %}
</div>
