/*
Copyright 2015-2016 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#include <QQuickItem>

#include "systeminteractor.h"

/*!
 * \brief The CdWriter class uses K3B to create an audio CD from the current
 * playlist. For this to work, K3B must be in the $PATH and it must be able to
 * decode the audio files.
 */
class CdWriter : public QQuickItem {
  Q_OBJECT

 public:
  CdWriter();
  ~CdWriter();

  Q_INVOKABLE bool hasK3b() const;
  Q_INVOKABLE bool burnList(const QStringList &pathList) const;

 private:
  SystemInteractor *sys_;
};
